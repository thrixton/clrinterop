using System;
using System.Collections.Generic;
using System.Text;
using CoreRuleEngine;
using System.Xml;

namespace TlbImpRuleEngine
{
    public class TlbImpActionManager : AbstractActionManager
    {
        public TlbImpActionManager()
        {
            ChangeManagedNameActionDef changeNameActionDef = ChangeManagedNameActionDef.GetInstance();
            RegisterAction(changeNameActionDef.GetActionName(),
                           changeNameActionDef);
            PreserveSigActionDef preserveSigActionDef = PreserveSigActionDef.GetInstance();
            RegisterAction(preserveSigActionDef.GetActionName(),
                           preserveSigActionDef);
            ResolveToActionDef resolveRedirectionActionDef =
                ResolveToActionDef.GetInstance();
            RegisterAction(resolveRedirectionActionDef.GetActionName(),
                           resolveRedirectionActionDef);

            ConvertToActionDef convertToActionDef = ConvertToActionDef.GetInstance();
            RegisterAction(convertToActionDef.GetActionName(), convertToActionDef);

            AddAttributeActionDef addAttributeActionDef = AddAttributeActionDef.GetInstance();
            RegisterAction(addAttributeActionDef.GetActionName(), addAttributeActionDef);
        }
    }
}
