using Microsoft.Win32;
using System;

namespace LicenceKeyGenerator
{
    class RegeditHelper
    {
        const string REGKEY_COGA = "HKEY_LOCAL_MACHINE\\SOFTWARE\\COGAPLEX\\LICENSEKEY";
        public static void Register(string valueName, object value)
        {
            RegistryKey openRegKey;
            try
            {
                Registry.SetValue(REGKEY_COGA, valueName, value);

            }
            catch (Exception e)
            {
                openRegKey = Registry.LocalMachine.CreateSubKey(REGKEY_COGA);
                openRegKey.SetValue(valueName, value);
                openRegKey.Close();
            }
        }
    }
}
