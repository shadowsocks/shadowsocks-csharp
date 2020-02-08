﻿using NLog;
using Shadowsocks.Controller.Hotkeys;
using Shadowsocks.Model;
using System;
using System.Windows.Forms;

namespace Shadowsocks.Controller
{
    static class HotkeyReg
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public static void RegAllHotkeys()
        {
            HotkeyConfig hotkeyConfig = Configuration.Load().hotkey;

            if (hotkeyConfig == null || !hotkeyConfig.RegHotkeysAtStartup)
            {
                return;
            }

            // if any of the hotkey reg fail, undo everything
            if (RegHotkeyFromString(hotkeyConfig.SwitchSystemProxy, "SwitchSystemProxyCallback")
                && RegHotkeyFromString(hotkeyConfig.SwitchSystemProxyMode, "SwitchSystemProxyModeCallback")
                && RegHotkeyFromString(hotkeyConfig.SwitchAllowLan, "SwitchAllowLanCallback")
                && RegHotkeyFromString(hotkeyConfig.ShowLogs, "ShowLogsCallback")
                && RegHotkeyFromString(hotkeyConfig.ServerMoveUp, "ServerMoveUpCallback")
                && RegHotkeyFromString(hotkeyConfig.ServerMoveDown, "ServerMoveDownCallback")
            )
            {
                // success
            }
            else
            {
                RegHotkeyFromString("", "SwitchSystemProxyCallback");
                RegHotkeyFromString("", "SwitchSystemProxyModeCallback");
                RegHotkeyFromString("", "SwitchAllowLanCallback");
                RegHotkeyFromString("", "ShowLogsCallback");
                RegHotkeyFromString("", "ServerMoveUpCallback");
                RegHotkeyFromString("", "ServerMoveDownCallback");
                MessageBox.Show(I18N.GetString("Register hotkey failed"), I18N.GetString("Shadowsocks"));
            }
        }

        public static bool RegHotkeyFromString(string hotkeyStr, string callbackName, Action<RegResult> onComplete = null)
        {
            Delegate _callback = HotkeyCallbacks.GetCallback(callbackName);
            if (_callback == null)
            {
                throw new Exception($"{callbackName} not found");
            }

            HotKeys.HotKeyCallBackHandler callback = _callback as HotKeys.HotKeyCallBackHandler;

            if (hotkeyStr.IsNullOrEmpty())
            {
                HotKeys.UnregExistingHotkey(callback);
                onComplete?.Invoke(RegResult.UnregSuccess);
                return true;
            }
            else
            {
                GlobalHotKey.HotKey hotkey = HotKeys.Str2HotKey(hotkeyStr);
                if (hotkey == null)
                {
                    logger.Error($"Cannot parse hotkey: {hotkeyStr}");
                    onComplete?.Invoke(RegResult.ParseError);
                    return false;
                }
                else
                {
                    bool regResult = (HotKeys.RegHotkey(hotkey, callback));
                    if (regResult)
                    {
                        onComplete?.Invoke(RegResult.RegSuccess);
                    }
                    else
                    {
                        onComplete?.Invoke(RegResult.RegFailure);
                    }
                    return regResult;
                }
            }
        }

        public enum RegResult
        {
            RegSuccess,
            RegFailure,
            ParseError,
            UnregSuccess,
            //UnregFailure
        }
    }
}
