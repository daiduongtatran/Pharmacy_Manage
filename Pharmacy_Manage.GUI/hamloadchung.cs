using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pharmacy_Manage.GUI;


namespace Pharmacy_Manage.GUI
{
    public static class hamloadchung
    {
        public static event Action ReloadAll;

        public static void Reload()
        {
            ReloadAll?.Invoke();
        }
    }
}
