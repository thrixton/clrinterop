using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using TlbImpRuleEngine;
using System.Diagnostics;
using System.Globalization;

namespace TlbImpRuleFileEditor
{
    public partial class AddAttributeDataInputHelper : Form
    {
        Type m_type;
        
        ConstructorInfo m_ctor;
        
        string m_data;

        List<Control> m_parameterControlList;

        const int TYPE_LABEL_X = 2;

        const int TYPE_LABEL_WIDTH = 95;

        const int TYPE_ITEM_HEIGHT = 25;

        const int TYPE_CONTROL_WIDTH = 150;

        const int TYPE_CONTROL_X = 100;

        public AddAttributeDataInputHelper(Type type, ConstructorInfo ctor, string data)
        {
            InitializeComponent();
            m_type = type;
            m_ctor = ctor;
            m_data = data;
        }

        internal string GetEncodingData()
        {
            return m_data;
        }

        private void AddAttributeDataInputHelper_Load(object sender, EventArgs e)
        {
            textBoxType.Text = m_type.FullName;
            textBoxCtor.Text = m_ctor.ToString();

            m_parameterControlList = new List<Control>();

            ParameterInfo[] parameters = m_ctor.GetParameters();
            for (int i = 0; i < parameters.Length; i++)
            {
                Type type = parameters[i].ParameterType;
                // Type Label
                Label label = new Label();
                label.Text = type.Name + ":";
                label.Location = new Point(TYPE_LABEL_X, i * TYPE_ITEM_HEIGHT + 1);
                label.Width = TYPE_LABEL_WIDTH;
                panelParameters.Controls.Add(label);
                // Input Item: TextBox or ComboBox
                if (type == typeof(Boolean))
                {
                    ComboBox combo = new ComboBox();
                    combo.Items.Add(Boolean.TrueString);
                    combo.Items.Add(Boolean.FalseString);
                    combo.SelectedIndex = 0;
                    combo.DropDownStyle = ComboBoxStyle.DropDownList;
                    combo.Location = new Point(TYPE_CONTROL_X, i * TYPE_ITEM_HEIGHT);
                    combo.Width = TYPE_CONTROL_WIDTH;
                    panelParameters.Controls.Add(combo);
                    m_parameterControlList.Add(combo);
                }
                else if (type.IsEnum)
                {
                    ComboBox combo = new ComboBox();
                    foreach (string enumName in Enum.GetNames(type))
                    {
                        combo.Items.Add(enumName);
                    }
                    if (combo.Items.Count > 0)
                        combo.SelectedIndex = 0;
                    combo.DropDownStyle = ComboBoxStyle.DropDownList;
                    combo.Location = new Point(TYPE_CONTROL_X, i * TYPE_ITEM_HEIGHT);
                    combo.Width = TYPE_CONTROL_WIDTH;
                    panelParameters.Controls.Add(combo);
                    m_parameterControlList.Add(combo);
                }
                else if (type == typeof(Type))
                {
                    TextBox text = new TextBox();
                    text.Location = new Point(TYPE_CONTROL_X, i * TYPE_ITEM_HEIGHT);
                    text.Width = TYPE_CONTROL_WIDTH;
                    panelParameters.Controls.Add(text);
                    m_parameterControlList.Add(text);
                    // Type Selector
                    ManagedTypeSelectButton button = new ManagedTypeSelectButton(text);
                    button.Location = new Point(TYPE_CONTROL_X + TYPE_CONTROL_WIDTH + 2, i * TYPE_ITEM_HEIGHT);
                    button.Text = "...";
                    button.Width = 28;
                    button.Height = 22;
                    panelParameters.Controls.Add(button);
                }
                else
                {
                    TextBox text = new TextBox();
                    text.Location = new Point(TYPE_CONTROL_X, i * TYPE_ITEM_HEIGHT);
                    text.Width = TYPE_CONTROL_WIDTH;
                    panelParameters.Controls.Add(text);
                    m_parameterControlList.Add(text);
                }
            }
            if (!(m_data.Equals("") || m_data.Equals("01 00 00 00")))
            {
                try
                {
                    ParseData();
                }
                catch (Exception)
                {
                    MessageBox.Show(Resource.FormatString("Wrn_ParseDataFailed", m_ctor.ToString()));
                }
            }
        }

        private void ParseData()
        {
            byte[] blob;
            int index = 0;
            if (AddAttributeAction.GetBlobByString(m_data, out blob))
            {
                // skip 01 00
                index += 2;
                ParameterInfo[] parameters = m_ctor.GetParameters();
                for (int i = 0; i < parameters.Length; i++)
                {
                    Type type = parameters[i].ParameterType;
                    string value = GetValue(blob, ref index, type);
                    if (m_parameterControlList[i] is ComboBox)
                    {
                        (m_parameterControlList[i] as ComboBox).SelectedItem = value;
                    }
                    else
                    {
                        m_parameterControlList[i].Text = value;
                    }
                }
            }
            else
            {
                throw new ParseDataFailedException();
            }
        }

        private string GetValue(byte[] blob, ref int index, Type type)
        {
            if (type == typeof(Int16))
            {
                Int16 i = BitConverter.ToInt16(blob, index);
                index += 2;
                return i.ToString(CultureInfo.InvariantCulture);
            }
            else if (type == typeof(Int32))
            {
                Int32 i = BitConverter.ToInt32(blob, index);
                index += 4;
                return i.ToString(CultureInfo.InvariantCulture);
            }
            else if (type == typeof(Int64))
            {
                Int64 i = BitConverter.ToInt64(blob, index);
                index += 8;
                return i.ToString(CultureInfo.InvariantCulture);
            }
            else if (type == typeof(UInt16))
            {
                UInt16 ui = BitConverter.ToUInt16(blob, index);
                index += 2;
                return ui.ToString(CultureInfo.InvariantCulture);
            }
            else if (type == typeof(UInt32))
            {
                UInt32 ui = BitConverter.ToUInt32(blob, index);
                index += 4;
                return ui.ToString(CultureInfo.InvariantCulture);
            }
            else if (type == typeof(UInt64))
            {
                UInt64 ui = BitConverter.ToUInt64(blob, index);
                index += 8;
                return ui.ToString(CultureInfo.InvariantCulture);
            }
            else if (type == typeof(Boolean))
            {
                Boolean b = BitConverter.ToBoolean(blob, index);
                index += 1;
                return b.ToString(CultureInfo.InvariantCulture);
            }
            else if (type == typeof(string) || type == typeof(Type))
            {
                int stringPackLen = GetStringPackLen(blob, ref index);
                string str = Encoding.UTF8.GetString(blob, index, stringPackLen);
                index += stringPackLen;
                return str;
            }
            else if (type.IsEnum)
            {
                uint value = BitConverter.ToUInt32(blob, index);
                index += 4;
                return Enum.GetName(type, value);
            }
            else
            {
                throw new ParseDataTypeUnsupportedException();
            }
        }

        private int GetStringPackLen(byte[] blob, ref int index)
        {
            if (blob[index] >= 0xC0)
            {
                int ret = (blob[index] & 0x3F);
                ret = (ret << 8) | blob[index + 1];
                ret = (ret << 8) | blob[index + 2];
                ret = (ret << 8) | blob[index + 3];
                index += 4;
                return ret;
            }
            else if (blob[index] >= 0x80)
            {
                int ret = (blob[index] & 0x7F);
                ret = (ret << 8) | blob[index + 1];
                index += 2;
                return ret;
            }
            else
            {
                int ret = blob[index];
                index += 1;
                return ret;
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Dispose();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            List<byte> blob = new List<byte>();
            blob.Add(0x01);
            blob.Add(0x00);
            ParameterInfo[] parameters = m_ctor.GetParameters();
            for (int i = 0; i < parameters.Length; i++)
            {
                Type type = parameters[i].ParameterType;
                string value;
                if (m_parameterControlList[i] is ComboBox)
                {
                    value = (m_parameterControlList[i] as ComboBox).SelectedItem.ToString();
                }
                else
                {
                    value = m_parameterControlList[i].Text;
                }
                if (!AddBlob(blob, type, value))
                {
                    MessageBox.Show(Resource.FormatString("Wrn_BadParameterInput", value, type.ToString()));
                    return;
                }
            }
            AppendNumNamedArgs(blob, 0);

            m_data = AddAttributeAction.GetStringByBlob(blob.ToArray());

            DialogResult = DialogResult.OK;
            Dispose();
        }

        private void AppendNumNamedArgs(List<byte> bytes, int len)
        {
            Debug.Assert(len < short.MaxValue);
            bytes.Add((byte)(len & 0xff));
            bytes.Add((byte)(len >> 8));
        }

        private bool AddBlob(List<byte> blob, Type type, string value)
        {
            try
            {
                if (type == typeof(Int16))
                {
                    Int16 i = Int16.Parse(value, CultureInfo.InvariantCulture);
                    blob.Add((byte)(i & 0xFF));
                    blob.Add((byte)((i >> 8) & 0xFF));
                }
                else if (type == typeof(Int32))
                {
                    Int32 i = Int32.Parse(value, CultureInfo.InvariantCulture);
                    blob.Add((byte)(i & 0xFF));
                    blob.Add((byte)((i >> 8) & 0xFF));
                    blob.Add((byte)((i >> 16) & 0xFF));
                    blob.Add((byte)((i >> 24) & 0xFF));
                }
                else if (type == typeof(Int64))
                {
                    Int64 i = Int64.Parse(value, CultureInfo.InvariantCulture);
                    blob.Add((byte)(i & 0xFF));
                    blob.Add((byte)((i >> 8) & 0xFF));
                    blob.Add((byte)((i >> 16) & 0xFF));
                    blob.Add((byte)((i >> 24) & 0xFF));
                    blob.Add((byte)((i >> 32) & 0xFF));
                    blob.Add((byte)((i >> 40) & 0xFF));
                    blob.Add((byte)((i >> 48) & 0xFF));
                    blob.Add((byte)((i >> 56) & 0xFF));
                }
                else if (type == typeof(UInt16))
                {
                    UInt16 i = UInt16.Parse(value, CultureInfo.InvariantCulture);
                    blob.Add((byte)(i & 0xFF));
                    blob.Add((byte)((i >> 8) & 0xFF));
                }
                else if (type == typeof(UInt32))
                {
                    UInt32 i = UInt32.Parse(value, CultureInfo.InvariantCulture);
                    blob.Add((byte)(i & 0xFF));
                    blob.Add((byte)((i >> 8) & 0xFF));
                    blob.Add((byte)((i >> 16) & 0xFF));
                    blob.Add((byte)((i >> 24) & 0xFF));
                }
                else if (type == typeof(UInt64))
                {
                    UInt64 i = UInt64.Parse(value, CultureInfo.InvariantCulture);
                    blob.Add((byte)(i & 0xFF));
                    blob.Add((byte)((i >> 8) & 0xFF));
                    blob.Add((byte)((i >> 16) & 0xFF));
                    blob.Add((byte)((i >> 24) & 0xFF));
                    blob.Add((byte)((i >> 32) & 0xFF));
                    blob.Add((byte)((i >> 40) & 0xFF));
                    blob.Add((byte)((i >> 48) & 0xFF));
                    blob.Add((byte)((i >> 56) & 0xFF));
                }
                else if (type == typeof(Boolean))
                {
                    Boolean b = Boolean.Parse(value);
                    if (b)
                        blob.Add(0x01);
                    else
                        blob.Add(0x00);
                }
                else if (type == typeof(string) || type == typeof(Type))
                {
                    byte[] utf8 = Encoding.UTF8.GetBytes(value);
                    AppendPackedLen(blob, utf8.Length);
                    blob.AddRange(utf8);
                }
                else if (type.IsEnum)
                {
                    string digitalString = Enum.Format(type, Enum.Parse(type, value), "d");
                    Int32 i = Int32.Parse(digitalString, CultureInfo.InvariantCulture);
                    blob.Add((byte)(i & 0xFF));
                    blob.Add((byte)((i >> 8) & 0xFF));
                    blob.Add((byte)((i >> 16) & 0xFF));
                    blob.Add((byte)((i >> 24) & 0xFF));
                }
                else
                {
                    throw new ParseDataTypeUnsupportedException();
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private void AppendPackedLen(List<byte> bytes, int len)
        {
            if (len <= 0x7F)
            {
                bytes.Add((byte)len);
                return;
            }

            if (len <= 0x3FFF)
            {
                bytes.Add((byte)((len >> 8) | 0x80));
                bytes.Add((byte)(len & 0xFF));
                return;
            }

            Debug.Assert(len <= 0x1FFFFFFF);

            bytes.Add((byte)((len >> 24) | 0xC0));
            bytes.Add((byte)((len >> 16) & 0xFF));
            bytes.Add((byte)((len >> 8) & 0xFF));
            bytes.Add((byte)(len & 0xFF));
        }
    }
}
