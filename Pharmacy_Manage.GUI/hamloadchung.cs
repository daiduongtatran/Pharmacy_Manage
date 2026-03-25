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
//B1: ĐẶT DÒNG NÀY: "hamloadchung.ReloadAll += LoadData;" VÀO TRONG CONSTRUCTOR CỦA FILE .CS VÀ THAY "LOADDATA" BẰNG TÊN HÀM LOADATA CỦA TÙY TRANG
/*
B2:
private void UserControl_Unloaded(object sender, RoutedEventArgs e)
{
    hamloadchung.ReloadAll -= LoadDdata;
}

CÁI NÀY ĐỂ TRÁNH BUG (MUST HAVE) ĐẶT DƯỚI CONSTRUCTOR, NHỚ THAY LOADATA, NHỚ THAY LOADATA

B3: TRONG FILE XAML THÊM DÒNG Unloaded="UserControl_Unloaded" ĐỂ B2 HOẠT ĐỘNG. VD:
<UserControl x:Class="Pharmacy_Manage.GUI.QLkhachhang_view"
             Unloaded="UserControl_Unloaded">

NHỚ THAY TÊN LOADATA
*/