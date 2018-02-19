//TODO: convert instance method to dynamic method is not work. When do the instanc call, use callvirt, 
//callvirt on dynamic method doesnt' work because dynamic method is static method, need to change callvirt to call
//if converting a instanc method to dynamic method call.

//Full Generic Support

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace ILReader
{
	public enum ExceptionHandler					
	{
		Try,
		Filter,
		Catch,
        	Finally,
		Fault,
		EndException,
		EndFilter
	}
	public class Index								
	{
		string Name;

		public Index() { Name = ""; }
		public Index(string name) { Name = name; }
		
		public override string ToString()
		{
			return Name;
		}
	}

	public class Target								
	{
		string Name;

		public Target() { Name = ""; }
		public Target(string name) { Name = name; }
		public override string ToString()
		{
			return Name;
		}
		public Label ToLabel(ArrayList Opcodes)
		{
			Opcode Op1 = new Opcode();
			foreach(Opcode OP in Opcodes)
			{
				if (Name == OP.Label) return OP.ILLabel;
			}
			return Op1.ILLabel;
		}
	}

	public class Opcode								
	{
		public string TargetString = "";		// This is the Opcodes target as a string (if it has a target)
		public string Name = "";			// The Opcode's IL name (ex. ldc.i4.0)
		public object ParamVal = "";			// The value of the parameter of this Opcode
		public Opcode Prefix;			// If this Opcode has a prefix, it's stored here
		public string Label;				// A string representation of the label preceeding this Opcode. Used by the IL Reader to track Opcodes
		public bool IsPrefix;			// This boolean indicates whether this Opcode is a prefix.
		public bool NeedsLabel;			// If another Opcode will be branching to this one, this will be set to true so that the emitter marks a label before this Opcode
		public Label ILLabel;			// The IL Label that has been (or will be) marked before this Opcode
		public Assembly workingAssm;		// The assembly that we are currently working on
		public Type workingType;
		public Opcode()
		{
			ParamVal = null;
			Prefix = null;
			IsPrefix = false;
			NeedsLabel = false;
		}
		public Opcode(string name)
		{
			Name = name;
			Prefix = null;
			IsPrefix = false;
			NeedsLabel = false;
		}
		
		public Opcode(string name, object paramval)
		{
			Name = name;
			ParamVal = paramval;
			Prefix = null;
			IsPrefix = false;
			NeedsLabel = false;
		}
		public Opcode(string name, ArrayList paramlist)
		{
			Name = name;
			ParamVal = paramlist;
			Prefix = null;
			IsPrefix = false;
			NeedsLabel = false;
		}
		
		public virtual string DisplayIL()
		{
			StringBuilder s = new StringBuilder();
			if (Label != null)
				s.Append(Label+":");
			s.Append(Name + " ");					// This appends the name
			if (ParamVal != null)					// This appends the parameter(s) if any
			{
				if (typeof(MethodBase).IsInstanceOfType(ParamVal))
				{
					MethodBase mb = (MethodBase)ParamVal;
					if (!mb.IsStatic)
						s.Append("instance ");
					s.Append(Opcode.PrintMethod(mb, workingAssm, workingType));
					
                        	}
				else if (typeof(ArrayList).IsInstanceOfType(ParamVal))
				{
					s.Append("(" + Environment.NewLine);
					int count = 0;
					foreach (Target T in (ArrayList)ParamVal)	// This is for a switch with several targets
						if ((++count) == ((ArrayList)ParamVal).Count)	
							s.Append("\t" + T.ToString() + ")");
						else s.Append("\t" + T.ToString() + "," + Environment.NewLine);
				} else s.Append(ParamVal.ToString());
			}
			return s.ToString();
		}
		public static string PrintTypeWithAssem(Type t, Assembly workingAssm)
		{
			string s = "";
			if (!t.IsPrimitive && (t != typeof(string)) && (t!=typeof(object)) &&  (t.Assembly != workingAssm))
						s = ("[" + t.Assembly.GetName().Name + "]");
			return (s + t);
			
		}
		public static string PrintMethod(MethodBase mb, Assembly workingAssm, Type workingType)
		{
			StringBuilder s = new StringBuilder();
			if (typeof(MethodInfo).IsInstanceOfType(mb))
			{
				s.Append(PrintTypeWithAssem(((MethodInfo)mb).ReturnType, workingAssm));
				s.Append(" ");
			}			
			if (mb.DeclaringType != null)
			{
				if (mb.DeclaringType.Assembly != workingAssm)
					s.Append("[" + mb.DeclaringType.Assembly.GetName().Name + "]");
				if (mb.DeclaringType != workingType)
					s.Append(mb.DeclaringType + "::"); // we don't do the check to see if the method is declared in the same body with the calling site
	                }
			s.Append(mb.Name);
			if (mb.IsGenericMethodDefinition)
			{
				Type[] ts = mb.GetGenericArguments(); // we must have at least one generic argument	
				s.Append(" <");
				for (int i = 0; i < ts.Length; i++)
				{
					s.Append("("); // we should always have some generic constrains
					Type[] cts = ts[i].GetGenericParameterConstraints();
					Array.Sort(cts, new StringCompare());
					for (int j = 0; j < cts.Length; j++)
					{
						s.Append(PrintTypeWithAssem(cts[j], workingAssm));
						if (j<cts.Length-1)
							s.Append(", ");
					}
					s.Append(") ");
					s.Append(ts[i]);
					if (i < (ts.Length-1))
						s.Append(", ");
				}
				s.Append(">");
			}
            else if (mb.IsGenericMethod)
            {
				s.Append(" <");
				Type[] param = mb.GetGenericArguments();
				for (int i = 0; i < param.Length; i++)
				{
					s.Append(PrintTypeWithAssem(param[i], workingAssm));
					if (i < (param.Length-1))
						s.Append(", ");
				}
				s.Append("> ");
			}
	                s.Append(" (");
	                int count = 0;
	                foreach (ParameterInfo pa in mb.GetParameters())
	                {
	                	if (pa.ParameterType.IsGenericParameter)
	              		{
		              		if (pa.ParameterType.DeclaringMethod != null)
		              			s.Append("!!" + pa.ParameterType.GenericParameterPosition + " " + pa.ParameterType);
		              		else s.Append("!" + pa.ParameterType.GenericParameterPosition + " " + pa.ParameterType);
		              		
		              	}
		              	else
		              	{
		              		s.Append(PrintTypeWithAssem(pa.ParameterType, workingAssm));
		               	}
		               	if ((++count)< mb.GetParameters().Length)
		               		s.Append(", ");
		               	else s.Append(")");
		        } 
	                if (count == 0) s.Append(")");
               	 	return s.ToString();
      		}

	}
	
	public class ExceptionInstruction: Opcode		
	{
		
		int Position;
		ExceptionHandler ExceptionHandlerType;
		Type CatchType;
		public ExceptionInstruction(int p, ExceptionHandler e, Type c)
		{
			Position = p;
			ExceptionHandlerType = e;
			CatchType = c;
			Name = null;
		}
		public override string DisplayIL()
		{
			switch (ExceptionHandlerType)
			{
				case ExceptionHandler.Try:
					return ".try" + Environment.NewLine +"{";
				case ExceptionHandler.Filter:
					return "}" + Environment.NewLine +"filter" + Environment.NewLine +"{";
				case ExceptionHandler.Catch:
					return "}" + Environment.NewLine + "catch " + CatchType.FullName + Environment.NewLine + "{";
				case ExceptionHandler.Finally:
					return "}" + Environment.NewLine + "finally" + Environment.NewLine + "{";
				case ExceptionHandler.EndException:
					return "}";
				case ExceptionHandler.Fault:
					return "}" + Environment.NewLine + "finally" + Environment.NewLine +"{";
				case ExceptionHandler.EndFilter:
					return "endfilter" + Environment.NewLine + "}" + Environment.NewLine +"{";
				default:
					Console.WriteLine("Error: Unknown exception handling call, no exception handler emitted");
					return "Error";
			};
		}

		public int ExceptionPos
		{
			get {return Position;}
		}

	}

	public class ILReader							
	{
		ArrayList MethodOpcodes;		// This arraylist will contain all the opcodes as they are read from the method body.
		MethodBase	InputMeth;			// This is the MethodInfo of the input method
		MethodBody methodBody; 	//the Method Body object of the input method.
		Module curModule; 	//this is the module of the input method.
		ArrayList ExceptionHandlers;// This keeps track of the exception blocks in a method.
		byte[] IL;	// This is the byte[] that will be parsed for IL.
		int pos, pos1;	// pos is tracks the position in the byte[], so that the Reader knows what to read next.
				// pos1 tracks the position, but stops at the beginning of every Opcode, so that labels may be made
				// based on the Opcodes position in the array.
		Assembly workingAssm;

		ArrayList StfldList = new ArrayList(); // This array is used to store the special opcodes "stfld" whose order can change over the run

		void MakeLabel(Opcode op)				
		{
			op.Label = string.Format("IL_{0:x4}", pos1);
		}
		void MakeLabel(Opcode op, bool label)
		{
			if (!label)
				op.Label = "IL_XXXX";
			else MakeLabel(op);
		}
		unsafe int Uncompress(ref byte* psig)		
		{
			byte[] bytes = new byte[4];
			if (*psig  < 128)
			{
				int i = *psig;
				psig++;
				return i;
			}
			else if (*psig < 192)
			{
				bytes[1] = (byte)((*psig)-128);
				psig++;
				bytes[0] = *psig;
				psig++;
				return(BitConverter.ToInt32(bytes,0));				
			}
			else
			{
				for (int j = 3; j >= 0; j--) {bytes[j] = *psig; psig++;}
				bytes[3] -= 192;
				return(BitConverter.ToInt32(bytes,0));
			}
		}

		unsafe Type ResolveType(ref byte* psig)		
		{
			int LocalType = Uncompress(ref psig);
			byte[] TokenBytes = new byte[4]; for(int qq= 0; qq<4; qq++){TokenBytes[qq]=0;}
			switch (LocalType)
			{
				case 1:
					return (Type.GetType("System.Void"));
				case 2:
					return (Type.GetType("System.Boolean"));
				case 3:
					return (Type.GetType("System.Char"));
				case 4:
					return (Type.GetType("System.SByte"));
				case 5:
					return (Type.GetType("System.Byte"));
				case 6:
					return (Type.GetType("System.Int16"));
				case 7:
					return (Type.GetType("System.UInt16"));
				case 8:
					return (Type.GetType("System.Int32"));
				case 9:
					return (Type.GetType("System.UInt32"));
				case 10:
					return (Type.GetType("System.Int64"));
				case 11:
					return (Type.GetType("System.UInt64"));
				case 12:
					return (Type.GetType("System.Single"));
				case 13:
					return (Type.GetType("System.Double"));
				case 14:
					return (Type.GetType("System.String"));
				case 15:
					Type t = ResolveType(ref psig);
					return Type.GetType(t.ToString()+"*");
				case 16:
					Type t1 = ResolveType(ref psig);
					return Type.GetType(t1.ToString()+"&");
				case 17:
					// TODO : Account for TypeSpec (Leading byte of 0x03)???
					LocalType = Uncompress(ref psig);
					if (LocalType%2 == 1)									//TypeRef
					{
						LocalType -= 1;
						LocalType = LocalType >> 2;
						TokenBytes[0] = (byte)LocalType;
						TokenBytes[3] = 1;
					}
					else
					{														//TypeDef
						LocalType = LocalType >> 2;
						TokenBytes[0] = (byte)LocalType;
						TokenBytes[3] = 2;
					}
					return (Type)curModule.ResolveMember(BitConverter.ToInt32(TokenBytes,0));
				case 18:
					// TODO : Account for TypeSpec (Leading byte of 0x03)???
					LocalType = Uncompress(ref psig);
					if (LocalType%2 == 1)									//TypeRef
					{
						LocalType -= 1;
						LocalType = LocalType >> 2;
						TokenBytes[0] = (byte)LocalType;
						TokenBytes[3] = 1;
					}
					else
					{			
						LocalType = LocalType >> 2;						//TypeDef
						TokenBytes[0] = (byte)LocalType;
						TokenBytes[3] = 2;
					}
					return (Type)curModule.ResolveMember(BitConverter.ToInt32(TokenBytes,0));
				case 20:
					string s = "";
					Type BaseType = ResolveType(ref psig);
					s += BaseType.ToString() + "[";
					int rank = Uncompress(ref psig);
					for (int r = 1; r < rank; r++) s+= ",";
					int boundsCount = Uncompress(ref psig);
					int[] bounds = new int[boundsCount];
					for(int k = 0; k < boundsCount; k++) bounds[k] = Uncompress(ref psig);
					int loCount  = Uncompress(ref psig);
					int[] lo = new int[loCount];
					for(int k = 0; k < loCount; k++) lo[k] = Uncompress(ref psig);
                    s += "]";
					return (Type.GetType(s));
				case 21:
					//Generic local
					Type GenType = ResolveType(ref psig);
					Type[] GenParamVals = new Type[Uncompress(ref psig)];
					for (int i = 0; i < GenParamVals.Length; i++)
					{
						GenParamVals[i] = ResolveType(ref psig);
					}
                    GenType = GenType.MakeGenericType(GenParamVals);
                    return GenType;
				case 24:
					return (Type.GetType("System.IntPtr"));
				case 25:
					return (Type.GetType("System.UIntPtr"));
				case 28:
					return (Type.GetType("System.Object"));
				case 29:
					Type t2 = ResolveType(ref psig);
					return (Type.GetType(t2.ToString()+"[]"));

					// TODO : FnPtr? "0x1b followed by full method signature
					// TODO : Array? "0x14 <type><rank><boundsCount><bound1>... <loCount><lo1>...
				default:
					Console.WriteLine("Error: Unrecognized local, " + ((byte)*psig).ToString("x"));
					return null;
			};
		}

		Opcode GetOpcode()							
		{
			if (pos < IL.Length)						// First we check to make sure that there are bytes left unread in the array
			{								// If this is not true, then we will return a blank Opcode
				pos1 = pos;						// We set pos1 (which tracks the beginning of the most recent Opcode) to be equal to the current position
				foreach(ExceptionInstruction Exception in ExceptionHandlers)
				{
					if (Exception.ExceptionPos == pos)
					{
						ExceptionHandlers.Remove(Exception);
						return Exception;
					}
				}
				int Tok = IL[pos];						// Here, we read read then next byte, and increment out position in the array.
				pos++;

				

				// This large switch statement has cases depending on the value of the byte that was read from the array. Each byte
				// corresponds to an Opcode. So, we create (and return) a new Opcode depending on the byte that was read.

				switch (Tok)
				{
					case 0:
						return OpcodeNoParamVal("nop");
							
					case 1:
						return OpcodeNoParamVal("break");
							
					case 2:
						return OpcodeNoParamVal("ldarg.0");
							
					case 3:
						return OpcodeNoParamVal("ldarg.1");
							
					case 4:
						return OpcodeNoParamVal("ldarg.2");
							
					case 5:
						return OpcodeNoParamVal("ldarg.3");
							
					case 6:
						return OpcodeNoParamVal("ldloc.0");
							
					case 7:
						return OpcodeNoParamVal("ldloc.1");
							
					case 8:
						return OpcodeNoParamVal("ldloc.2");
							
					case 9:
						return OpcodeNoParamVal("ldloc.3");
							
					case 10:
						return OpcodeNoParamVal("stloc.0");
							
					case 11:
						return OpcodeNoParamVal("stloc.1");
							
					case 12:
						return OpcodeNoParamVal("stloc.2");
							
					case 13:
						return OpcodeNoParamVal("stloc.3");
							
					case 14:
						return OpcodeOneByte("ldarg.s");
							
					case 15:
						return OpcodeOneByte("ldarga.s");
							
					case 16:
						return OpcodeOneByte("starg.s");
							
					case 17:
						return OpcodeIndxS("ldloc.s");
							
					case 18:
						return OpcodeIndxS("ldloca.s");
							
					case 19:
						return OpcodeIndxS("stloc.s");
							
					case 20:
						return OpcodeNoParamVal("ldnull");
							
					case 21:
						return OpcodeNoParamVal("ldc.i4.m1");
							
					case 22:
						return OpcodeNoParamVal("ldc.i4.0");
							
					case 23:
						return OpcodeNoParamVal("ldc.i4.1");
							
					case 24:
						return OpcodeNoParamVal("ldc.i4.2");
							
					case 25:
						return OpcodeNoParamVal("ldc.i4.3");
							
					case 26:
						return OpcodeNoParamVal("ldc.i4.4");
							
					case 27:
						return OpcodeNoParamVal("ldc.i4.5");
							
					case 28:
						return OpcodeNoParamVal("ldc.i4.6");
							
					case 29:
						return OpcodeNoParamVal("ldc.i4.7");
							
					case 30:
						return OpcodeNoParamVal("ldc.i4.8");
							
					case 31:
						return OpcodeOneByte("ldc.i4.s");
							
					case 32:
						return OpcodeFourByte("ldc.i4");
							
					case 33:
						return OpcodeEightByte("ldc.i8");
							
					case 34:
						return OpcodeFourByteF("ldc.r4");
							
					case 35:
						return OpcodeEightByteF("ldc.r8");
							
					case 37:
						return OpcodeNoParamVal("dup");
							
					case 38:
						return OpcodeNoParamVal("pop");
							
					case 39:
						return OpcodeMeth("jmp");
							
					case 40:
						return OpcodeMethCall("call");
							
					case 41:
						return OpcodeSig("calli");
							
					case 42:
						return OpcodeNoParamVal("ret");
							
					case 43:
						return OpcodeTargetS("br.s");
							
					case 44:
						return OpcodeTargetS("brfalse.s");
							
					case 45:
						return OpcodeTargetS("brtrue.s");
							
					case 46:
						return OpcodeTargetS("beq.s");
							
					case 47:
						return OpcodeTargetS("bge.s");
							
					case 48:
						return OpcodeTargetS("bgt.s");
							
					case 49:
						return OpcodeTargetS("ble.s");
							
					case 50:
						return OpcodeTargetS("blt.s");
							
					case 51:
						return OpcodeTargetS("bne.un.s");
							
					case 52:
						return OpcodeTargetS("bge.un.s");
							
					case 53:
						return OpcodeTargetS("bgt.un.s");
							
					case 54:
						return OpcodeTargetS("ble.un.s");
							
					case 55:
						return OpcodeTargetS("blt.un.s");
							
					case 56:
						return OpcodeTarget("br");
							
					case 57:
						return OpcodeTarget("brfalse");
							
					case 58:
						return OpcodeTarget("brtrue");
							
					case 59:
						return OpcodeTarget("beq");
							
					case 60:
						return OpcodeTarget("bge");
							
					case 61:
						return OpcodeTarget("bgt");
							
					case 62:
						return OpcodeTarget("ble");
							
					case 63:
						return OpcodeTarget("blt");
							
					case 64:
						return OpcodeTarget("bne.un");
							
					case 65:
						return OpcodeTarget("bge.un");
							
					case 66:
						return OpcodeTarget("bgt.un");
							
					case 67:
						return OpcodeTarget("ble.un");
							
					case 68:
						return OpcodeTarget("blt.un");
							
					case 69:
						return OpcodeSwitch("switch");
							
					case 70:
						return OpcodeNoParamVal("ldind.i1");
							
					case 71:
						return OpcodeNoParamVal("ldind.u1");
							
					case 72:
						return OpcodeNoParamVal("ldind.i2");
							
					case 73:
						return OpcodeNoParamVal("ldind.u2");
							
					case 74:
						return OpcodeNoParamVal("ldind.i4");
							
					case 75:
						return OpcodeNoParamVal("ldind.u4");
							
					case 76:
						return OpcodeNoParamVal("ldind.i8");
							
					case 77:
						return OpcodeNoParamVal("ldind.i");
							
					case 78:
						return OpcodeNoParamVal("ldind.r4");
							
					case 79:
						return OpcodeNoParamVal("ldind.r8");
							
					case 80:
						return OpcodeNoParamVal("ldind.ref");
							
					case 81:
						return OpcodeNoParamVal("stind.ref");
							
					case 82:
						return OpcodeNoParamVal("stind.i1");
							
					case 83:
						return OpcodeNoParamVal("stind.i2");
							
					case 84:
						return OpcodeNoParamVal("stind.i4");
							
					case 85:
						return OpcodeNoParamVal("stind.i8");
							
					case 86:
						return OpcodeNoParamVal("stind.r4");
							
					case 87:
						return OpcodeNoParamVal("stind.r8");
							
					case 88:
						return OpcodeNoParamVal("add");
							
					case 89:
						return OpcodeNoParamVal("sub");
							
					case 90:
						return OpcodeNoParamVal("mul");
							
					case 91:
						return OpcodeNoParamVal("div");
							
					case 92:
						return OpcodeNoParamVal("div.un");
							
					case 93:
						return OpcodeNoParamVal("rem");
							
					case 94:
						return OpcodeNoParamVal("rem.un");
							
					case 95:
						return OpcodeNoParamVal("and");
							
					case 96:
						return OpcodeNoParamVal("or");
							
					case 97:
						return OpcodeNoParamVal("xor");
							
					case 98:
						return OpcodeNoParamVal("shl");
							
					case 99:
						return OpcodeNoParamVal("shr");
							
					case 100:
						return OpcodeNoParamVal("shr.un");
							
					case 101:
						return OpcodeNoParamVal("neg");
							
					case 102:
						return OpcodeNoParamVal("not");
							
					case 103:
						return OpcodeNoParamVal("conv.i1");
							
					case 104:
						return OpcodeNoParamVal("conv.i2");
							
					case 105:
						return OpcodeNoParamVal("conv.i4");
							
					case 106:
						return OpcodeNoParamVal("conv.i8");
							
					case 107:
						return OpcodeNoParamVal("conv.r4");
							
					case 108:
						return OpcodeNoParamVal("conv.r8");
							
					case 109:
						return OpcodeNoParamVal("conv.u4");
							
					case 110:
						return OpcodeNoParamVal("conv.u8");
							
					case 111:
						return OpcodeMethCall("callvirt");
							
					case 112:
						return OpcodeType("cpobj");
							
					case 113:
						return OpcodeType("ldobj");
							
					case 114:
						return OpcodeStr("ldstr");
							
					case 115:
						return OpcodeMeth("newobj");
							
					case 116:
						return OpcodeType("castclass");
							
					case 117:
						return OpcodeType("isinst");
							
					case 118:
						return OpcodeNoParamVal("conv.r.un");
							
					case 121:
						return OpcodeType("unbox");
							
					case 122:
						return OpcodeNoParamVal("throw");
							
					case 123:
						return OpcodeFld("ldfld");
							
					case 124:
						return OpcodeFld("ldflda");
							
					case 125:
						return OpcodeFld("stfld");
							
					case 126:
						return OpcodeFld("ldsfld");
							
					case 127:
						return OpcodeFld("ldsflda");
							
					case 128:
						return OpcodeFld("stsfld");
							
					case 129:
						return OpcodeType("stobj");
							
					case 130:
						return OpcodeNoParamVal("conv.ovf.i1.un");
							
					case 131:
						return OpcodeNoParamVal("conv.ovf.i2.un");
							
					case 132:
						return OpcodeNoParamVal("conv.ovf.i4.un");
							
					case 133:
						return OpcodeNoParamVal("conv.ovf.i8.un");
							
					case 134:
						return OpcodeNoParamVal("conv.ovf.u1.un");
							
					case 135:
						return OpcodeNoParamVal("conv.ovf.u2.un");
							
					case 136:
						return OpcodeNoParamVal("conv.ovf.u4.un");
							
					case 137:
						return OpcodeNoParamVal("conv.ovf.u8.un");
							
					case 138:
						return OpcodeNoParamVal("conv.ovf.i.un");
							
					case 139:
						return OpcodeNoParamVal("conv.ovf.u.un");
							
					case 140:
						return OpcodeType("box");
							
					case 141:
						return OpcodeType("newarr");
							
					case 142:
						return OpcodeNoParamVal("ldlen");
							
					case 143:
						return OpcodeType("ldelema");
							
					case 144:
						return OpcodeNoParamVal("ldelem.i1");
							
					case 145:
						return OpcodeNoParamVal("ldelem.u1");
							
					case 146:
						return OpcodeNoParamVal("ldelem.i2");
							
					case 147:
						return OpcodeNoParamVal("ldelem.u2");
							
					case 148:
						return OpcodeNoParamVal("ldelem.i4");
							
					case 149:
						return OpcodeNoParamVal("ldelem.u4");
							
					case 150:
						return OpcodeNoParamVal("ldelem.i8");
							
					case 151:
						return OpcodeNoParamVal("ldelem.i");
							
					case 152:
						return OpcodeNoParamVal("ldelem.r4");
							
					case 153:
						return OpcodeNoParamVal("ldelem.r8");
							
					case 154:
						return OpcodeNoParamVal("ldelem.ref");
							
					case 155:
						return OpcodeNoParamVal("stelem.i");
							
					case 156:
						return OpcodeNoParamVal("stelem.i1");
							
					case 157:
						return OpcodeNoParamVal("stelem.i2");
							
					case 158:
						return OpcodeNoParamVal("stelem.i4");
							
					case 159:
						return OpcodeNoParamVal("stelem.i8");
							
					case 160:
						return OpcodeNoParamVal("stelem.r4");
							
					case 161:
						return OpcodeNoParamVal("stelem.r8");
							
					case 162:
						return OpcodeNoParamVal("stelem.ref");
							
					case 179:
						return OpcodeNoParamVal("conv.ovf.i1");
							
					case 180:
						return OpcodeNoParamVal("conv.ovf.u1");
							
					case 181:
						return OpcodeNoParamVal("conv.ovf.i2");
							
					case 182:
						return OpcodeNoParamVal("conv.ovf.u2");
							
					case 183:
						return OpcodeNoParamVal("conv.ovf.i4");
							
					case 184:
						return OpcodeNoParamVal("conv.ovf.u4");
							
					case 185:
						return OpcodeNoParamVal("conv.ovf.i8");
							
					case 186:
						return OpcodeNoParamVal("conv.ovf.u8");
							
					case 194:
						return OpcodeType("refanyval");
							
					case 195:
						return OpcodeNoParamVal("ckfinite");
							
					case 198:
						return OpcodeType("mkrefany");
							
					case 208:
						return OpcodeTok("ldtoken");
							
					case 209:
						return OpcodeNoParamVal("conv.u2");
							
					case 210:
						return OpcodeNoParamVal("conv.u1");
							
					case 211:
						return OpcodeNoParamVal("conv.i");
							
					case 212:
						return OpcodeNoParamVal("conv.ovf.i");
							
					case 213:
						return OpcodeNoParamVal("conv.ovf.u");
							
					case 214:
						return OpcodeNoParamVal("add.ovf");
							
					case 215:
						return OpcodeNoParamVal("add.ovf.un");
							
					case 216:
						return OpcodeNoParamVal("mul.ovf");
							
					case 217:
						return OpcodeNoParamVal("mul.ovf.un");
							
					case 218:
						return OpcodeNoParamVal("sub.ovf");
							
					case 219:
						return OpcodeNoParamVal("sub.ovf.un");
							
					case 220:
						return OpcodeNoParamVal("endfinally");
							
					case 221:
						return OpcodeTarget("leave");
							
					case 222:
						return OpcodeTargetS("leave.s");
							
					case 223:
						return OpcodeNoParamVal("stind.i");
							
					case 224:
						return OpcodeNoParamVal("conv.u");
							
					case 254:								// If the byte FE (254) is read, then then next parameter determines the Opcode
						int Tok2 = IL[pos];					// This nested switch statement handles that.
						pos++;								// pos1 is not changed because the position of the FE byte is still considered the
					switch (Tok2)						// beginning of the Opcode token.
					{
						case 0:
							return OpcodeNoParamVal("arglist");
									
						case 1:
							return OpcodeNoParamVal("ceq");
									
						case 2:
							return OpcodeNoParamVal("cgt");
									
						case 3:
							return OpcodeNoParamVal("cgt.un");
									
						case 4:
							return OpcodeNoParamVal("clt");
									
						case 5:
							return OpcodeNoParamVal("clt.un");
									
						case 6:
							return OpcodeMeth("ldftn");
									
						case 7:
							return OpcodeMeth("ldvirtftn");
									
						case 9:
							return OpcodeTwoByte("ldarg");
									
						case 10:
							return OpcodeTwoByte("ldarga");
									
						case 11:
							return OpcodeTwoByte("starg");
									
						case 12:
							return OpcodeIndx("ldloc");
									
						case 13:
							return OpcodeIndx("ldloca");
									
						case 14:
							return OpcodeIndx("stloc");
									
						case 15:
							return OpcodeNoParamVal("localloc");
									
						case 17:
							return OpcodeNoParamVal("nop");
									
						case 18:
							return PrefixParamVal("unaligned.");
									
						case 19:
							return Prefix("volatile.");
									
						case 20:
							return Prefix("tail.");
									
						case 21:
							return OpcodeType("initobj");
									
						case 23:
							return OpcodeNoParamVal("cpblk");
									
						case 24:
							return OpcodeNoParamVal("initblk");
									
						case 26:
							return OpcodeNoParamVal("rethrow");
									
						case 28:
							return OpcodeType("sizeof");
									
						case 29:
							return OpcodeNoParamVal("refanytype");
									
						default:
							Console.WriteLine(Environment.NewLine + "ERROR - Invalid byte (" + Tok2.ToString("x") + "). Invalid IL byte stream.");
							return new Opcode();
					};
							
					default:
						Console.WriteLine(Environment.NewLine + "ERROR - Invalid byte (" + Tok.ToString("x") + "). Invalid IL byte stream.");
						return new Opcode();
				};

				

			}
			else return new Opcode();
		}

		
		Opcode OpcodeNoParamVal(string s)				
		{
			Opcode NewOp =  new Opcode(s);
			MakeLabel(NewOp);
			return NewOp;
		}
		Opcode OpcodeOneByte(string s)				
		{
			// This gets the parameter (stored as b) from the byte[]
			byte b = IL[pos];
			pos++;
			Opcode NewOp =  new Opcode(s,b);
			MakeLabel(NewOp);
			return NewOp;
		}
		Opcode OpcodeIndxS(string s)				
		{
			// This gets the parameter (stored as b) from the byte[]
			byte b = IL[pos];
			pos++; 
			Index ind = new Index("V_" + b);
			Opcode NewOp =  new Opcode(s,b);		
			MakeLabel(NewOp);	
			return NewOp;
		}
		Opcode OpcodeIndx(string s)					
		{
			// This gets the parameter (stored as b) from the byte[]
			byte[] b = new byte[2];
			for (int k = 0; k < 2; k++)
			{
				b[k] = IL[pos];
				pos++;
			}
			short i = BitConverter.ToInt16(b,0);
			Index ind = new Index("V_" + i);

			Opcode NewOp =  new Opcode(s,i);
			MakeLabel(NewOp);
			return NewOp;
		}
		Opcode OpcodeTwoByte(string s)				
		{
			// This gets the parameter (stored as b) from the byte[]
			byte[] b = new byte[2];
			for (int k = 0; k < 2; k++)
			{
				b[k] = IL[pos];
				pos++;
			}
			ushort i = BitConverter.ToUInt16(b,0);

			Opcode NewOp =  new Opcode(s,i);
			MakeLabel(NewOp);								// This labels the Opcode
			return NewOp;
		}

		Opcode OpcodeFourByteF(string s)			
		{
			// This gets the parameter (stored as b) from the byte[]
			byte[] b = new byte[8];
			for (int k = 0; k < 4; k++)
			{
				b[k] = IL[pos];
				pos++;
			}

			Opcode NewOp =  new Opcode(s,BitConverter.ToDouble(b,0));
			MakeLabel(NewOp);								// This labels the Opcode
			return NewOp;
		}
		Opcode OpcodeFourByte(string s)				
		{
			// This gets the parameter (stored as b) from the byte[]
			byte[] b = new byte[4];
			for (int k = 0; k < 4; k++)
			{
				b[k] = IL[pos];
				pos++;
			}
			int i = BitConverter.ToInt32(b,0);

			Opcode NewOp =  new Opcode(s,i);
			MakeLabel(NewOp);
			return NewOp;
		}
		Opcode OpcodeEightByteF(string s)			
		{
			// This gets the parameter (stored as b) from the byte[]
			byte[] b = new byte[8];
			for (int k = 0; k < 8; k++)
			{
				b[k] = IL[pos];
				pos++;
			}
			Opcode NewOp =  new Opcode(s,BitConverter.ToDouble(b,0));
			MakeLabel(NewOp);			
			return NewOp;
		}
		Opcode OpcodeEightByte(string s)			
		{
			// This gets the parameter (stored as b) from the byte[]
			byte[] b = new byte[8];
			for (int k = 0; k < 8; k++)
			{
				b[k] = IL[pos];
				pos++;
			}
			long i = BitConverter.ToInt64(b,0);

			Opcode NewOp =  new Opcode(s,i);
			MakeLabel(NewOp);
			return NewOp;
		}
		Opcode OpcodeMeth(string s)					
		{
			// This gets the parameter (stored as b) from the byte[]
			byte[] b = new byte[4];
			Array.Copy(IL, pos, b, 0, 4);
			pos+=4;
			MethodBase mbase = (MethodBase)curModule.ResolveMember(BitConverter.ToInt32(b,0));
			Opcode NewOp = new Opcode(s, mbase);
			NewOp.workingAssm = this.workingAssm;
			NewOp.workingType = this.InputMeth.DeclaringType;
			MakeLabel(NewOp);															// This labels the Opcode
			return NewOp;
		}

		Opcode OpcodeMethCall(string s)				
		{
			Opcode NewOp = OpcodeMeth(s);
			return NewOp;
		}

		Opcode OpcodeSig(string s)					
		{
			// TODO : Resolve this token!
			// This gets the parameter (stored as b) from the byte[]
			byte[] b = new byte[4];
			for (int k = 0; k < 4; k++)
			{
				b[k] = IL[pos];
				pos++;
			}
			Opcode NewOp =  new Opcode(s,b);
			MakeLabel(NewOp);
			return NewOp;
		}
		Opcode OpcodeTargetS(string s)				
		{
			// This gets the parameter (stored as b) from the byte[]
			sbyte k = (sbyte)IL[pos];
			pos++;
			int b = (int)((sbyte)k + (int)pos);
			string s1 =string.Format("IL_{0:x4}", b);	
			Opcode NewOp =  new Opcode(s,new Target(s1));
			MakeLabel(NewOp);
			return NewOp;
		}
		Opcode OpcodeTarget(string s)				
		{
			// This gets the parameter (stored as b) from the byte[]
			byte[] b  = new byte[4];
			for (int k = 0; k < 4; k++)
			{
				b[k] = IL[pos];
				pos++;
			}
			int l = BitConverter.ToInt32(b,0);
			int TPos = l + pos;					
			string s1 =string.Format("IL_{0:x4}", TPos);
			Opcode NewOp =  new Opcode(s,new Target(s1));
			MakeLabel(NewOp);
			return NewOp;
		}

		Opcode OpcodeType(string s)					
		{
			// This gets the parameter (stored as b) from the byte[]
			byte[] b = new byte[4];
			for (int k = 0; k < 4; k++)
			{
				b[k] = IL[pos];
				pos++;
			}
			Type RetType = (Type)curModule.ResolveMember(BitConverter.ToInt32(b,0));

			Opcode NewOp =  new Opcode(s,RetType);
			MakeLabel(NewOp);
			return NewOp;
		}
 		Opcode OpcodeStr(string s)					
		{
			// This gets the parameter (stored as b) from the byte[]
			byte[] b = new byte[4];
			for (int k = 0; k < 4; k++)
			{
				b[k] = IL[pos];
				pos++;
			}
			string str = curModule.ResolveString(BitConverter.ToInt32(b,0));

			Opcode NewOp =  new Opcode(s,str);
			MakeLabel(NewOp);
			return NewOp;
		}
		Opcode OpcodeFld(string s)					
		{			
			// This gets the parameter (stored as b) from the byte[]
			byte[] b = new byte[4];
			for (int k = 0; k < 4; k++)
			{
				b[k] = IL[pos];
				pos++;
			}
			// We have to temporarily take out stfld field here.
			// the stfld's order must be fixed up
			FieldInfo ReturnField = (FieldInfo)curModule.ResolveMember(BitConverter.ToInt32(b,0));
			Opcode NewOp =  new Opcode(s,ReturnField);
			if (s.Equals("stfld"))
			{
				StfldList.Add(NewOp.ParamVal);
				return NewOp;
			}
			else
			{
				MakeLabel(NewOp);
				return NewOp;
			}
		}

		Opcode OpcodeTok(string s)					
		{																
			// This gets the parameter (stored as b) from the byte[]
			byte[] b = new byte[4];
			for (int k = 0; k < 4; k++)
			{
				b[k] = IL[pos];
				pos++;
			}
			// This calls the appropriate Opcode method depending on what sort of parameter we're dealing with.
			// This is determined by the least byte of the byte[]
			if (b[3] == 4)
			{
				pos -=4;
				return OpcodeFld(s);
			}
			if ((b[3] == 6)||(b[3] == 10))
			{
				pos -=4;
				return OpcodeMeth(s);
			}
			if ((b[3] == 1)||(b[3]==2) || (b[3]==0x1b)) //0x1b is testspec.
			{
				pos -=4;
				return OpcodeType(s);
			}
			Opcode NewOp =  new Opcode(s,b);
			MakeLabel(NewOp);
			return NewOp;
		}

		Opcode OpcodeSwitch(string s)				
		{
			// This gets the number of parameters (stored as b) from the byte[]
			// Each parameter is then added to the ArrayList T
			ArrayList T = new ArrayList();
			byte[] b = new byte[4];
			for (int k = 0; k < 4; k++)
			{
				b[k] = IL[pos];
				pos++;
			}
			int i = BitConverter.ToInt32(b,0);				// This tells us how many parameters to expect.
			for (int k = 0; k < i; k++)						// Each parameter will be a four-byte offset
			{
				for (int k1 = 0; k1 < 4; k1++)
				{
					b[k1] = IL[pos];
					pos++;
				}
				int j = BitConverter.ToInt32(b,0);
				string s1 =string.Format("IL_{0:x4}", pos1 + j + 4*i + 5);
				T.Add(new Target(s1));
			}
			Opcode NewOp =  new Opcode(s,T);
			MakeLabel(NewOp);
			return NewOp;
		}
		Opcode Prefix(string s)				
		{
			Opcode OpPre = new Opcode(s);
			MakeLabel(OpPre);							// This labels the Opcode
			OpPre.IsPrefix = true;							// Here we designate the Opcode as a prefix
			Opcode Next = GetOpcode();
			Next.Label = "";
			Next.Prefix = OpPre;
			return Next;
		}
		Opcode PrefixParamVal(string s)			
		{
			// This gets the parameter (stored as b) from the byte[]
			byte b = IL[pos];
			pos++;
			Opcode OpPre = new Opcode(s,b);
			MakeLabel(OpPre);							// This labels the Opcode
			OpPre.IsPrefix = true;							// Here we designate the Opcode as a prefix
			Opcode Next = GetOpcode();
			Next.Label = "";
			Next.Prefix = OpPre;
			return Next;
		}
		public ILReader(MethodBase InputMethod, Assembly currentAssm):this(InputMethod,new FieldInfo[0],new MethodBase[0], currentAssm)
		{}
		
		public ILReader(MethodBase InputMethod, FieldInfo[] FieldsInModule, MethodBase[] MethodsInModule, Assembly currentAssm)	
		{
			
			workingAssm = currentAssm;
			MethodOpcodes = new ArrayList();
			InputMeth = InputMethod;
			ExceptionHandlers = new ArrayList();

			if (InputMethod == null) 
			{
				Console.WriteLine("Error, input method is not specified, can not continue");
				return;
			}
			methodBody = InputMethod.GetMethodBody();
			curModule = InputMethod.DeclaringType.Module;
					
			pos = 0;				
			pos1 = 0;
			
			IL = methodBody.GetILAsByteArray();
			
			IList<ExceptionHandlingClause>  ehClauses = methodBody.ExceptionHandlingClauses;
			foreach( ExceptionHandlingClause ehclause in ehClauses )
			{
				ExceptionHandlers.Add(new ExceptionInstruction(ehclause.TryOffset,ExceptionHandler.Try,null));
				switch (ehclause.Flags)
				{
					//case 0:
					case ExceptionHandlingClauseOptions.Clause:
						ExceptionHandlers.Add(new ExceptionInstruction(ehclause.HandlerOffset,ExceptionHandler.Catch,ehclause.CatchType));
						ExceptionHandlers.Add(new ExceptionInstruction(ehclause.HandlerOffset+ehclause.HandlerLength,ExceptionHandler.EndException,null));
					break;
					//case 1:
				case ExceptionHandlingClauseOptions.Filter:
					ExceptionHandlers.Add(new ExceptionInstruction(ehclause.FilterOffset,ExceptionHandler.Filter,null));
						ExceptionHandlers.Add(new ExceptionInstruction(ehclause.HandlerOffset,ExceptionHandler.EndFilter,null));
						ExceptionHandlers.Add(new ExceptionInstruction(ehclause.HandlerOffset+ehclause.HandlerLength,ExceptionHandler.EndException,null));
						break;
					//case 2:
					case ExceptionHandlingClauseOptions.Finally:
						ExceptionHandlers.Add(new ExceptionInstruction(ehclause.HandlerOffset,ExceptionHandler.Finally,null));
						ExceptionHandlers.Add(new ExceptionInstruction(ehclause.HandlerOffset+ehclause.HandlerLength,ExceptionHandler.EndException,null));
						break;
					//case 4:
					case ExceptionHandlingClauseOptions.Fault:
						ExceptionHandlers.Add(new ExceptionInstruction(ehclause.HandlerOffset,ExceptionHandler.Fault,null));
						ExceptionHandlers.Add(new ExceptionInstruction(ehclause.HandlerOffset+ehclause.HandlerLength,ExceptionHandler.EndException,null));
						break;
				};
			}
			// populate opcode
			Opcode Op = GetOpcode();
			while (Op.Name != "")												// This cycles through all the Opcodes
			{
				if (Op.Name != "stfld")
					MethodOpcodes.Add(Op);
				Op = GetOpcode();
			}
			// sort the stfld opcodes and output
			// this will make the opcode invalid for run. 
			// but out purpose is simply compare the assemblies.
			StfldList.Sort(new StringCompare());
			foreach (object i in StfldList)
			{
				Opcode temp = new Opcode("stfld", i);
				MakeLabel(temp, false);
				MethodOpcodes.Add(temp);
			}
		}

		public ArrayList GetOpcodes()
		{
			return MethodOpcodes;
		}
		
	}
	public class StringCompare : IComparer  
	{
	      	int IComparer.Compare( Object x, Object y )  
	      	{
	          	if (x.ToString().Equals(y.ToString()))
	          		return 1;
	          	else return string.Compare(x.ToString(),y.ToString());
	      	}
   	}
}
