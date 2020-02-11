﻿using System;

namespace IVPN
{
    public class LoginItems
    {
        private const string SCRIPT_FILE = "osascript";

        private const string LOGIN_ITEM_NAME = "IVPN";
        private const string LOGIN_ITEM_PATH = "/Applications/IVPN.app";

        private const string GET_LOGIN_ITEMS = 
            "-e 'tell application \"System Events\" to get the name of every login item'";

        private const string ADD_LOGIN_ITEM = 
            "-e 'tell application \"System Events\" to make login item at end " + 
            "with properties {{name: \"{0}\",path:\"{1}\", hidden:{2}}}'";

        private const string REMOVE_LOGIN_ITEM = 
            "-e 'tell application \"System Events\" to delete login item \"{0}\"'";

        
        public LoginItems()
        {
            
        }

        public static void AddLoginItem(bool isHidden = false)
        {
			if ( IsLoginItemInstalled() )
				return;

            Shell.ShellCommand.RunCommand(
                SCRIPT_FILE, 
                String.Format(ADD_LOGIN_ITEM, LOGIN_ITEM_PATH, LOGIN_ITEM_PATH, (isHidden)?"true":"false")
            );
        }

        public static void RemoveLoginItem()
        {
            if ( !IsLoginItemInstalled() )
                return;

            Shell.ShellCommand.RunCommand(
                SCRIPT_FILE, 
                String.Format(REMOVE_LOGIN_ITEM, LOGIN_ITEM_NAME)
            );
        }

        public static bool IsLoginItemInstalled()
        {
            var runCommandResult = Shell.ShellCommand.RunCommand(
                                       SCRIPT_FILE, GET_LOGIN_ITEMS);

            if (runCommandResult.IsAborted)
                return false;

            if (runCommandResult.ExitCode != 0)
                throw new Exception($"Unable to check LoginItem: {runCommandResult.ExitCode} {runCommandResult.ErrorOutput}");

            var loginItems = runCommandResult.Output.Split(new char[] { ',' });
            foreach (var loginItem in loginItems) {
                if (loginItem.Trim() == LOGIN_ITEM_NAME)
                    return true;                    
            }

            return false;
        }
    }
}

