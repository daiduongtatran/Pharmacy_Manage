using Microsoft.Data.SqlClient;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Data;
using System.Globalization; 
using System.IO;
using System.Windows.Data; 
using System.Windows.Media.Imaging;
using System.Collections.Generic;

namespace Pharmacy_Manage.GUI.KhachHang
{
    public partial class StoreView : UserControl
    {
        private string connectionString = @"Data Source=localhost;Initial Catalog=PharmacyManage;Integrated Security=True;TrustServerCertificate=True";

        public ObservableCollection<Product> ProductsList { get; set; }
        public ObservableCollection<Product> FilteredProducts { get; set; }
        public ObservableCollection<CartItem> CartList { get; set; }
        
        private string currentCategory = "Tất cả";

        public StoreView()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            ProductsList = new ObservableCollection<Product>();
            CartList = new ObservableCollection<CartItem>();
            FilteredProducts = new ObservableCollection<Product>(); 

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT MaSP, TenSP, GiaBan, HinhAnh, LoaiSP, TonKho FROM SanPham WHERE TrangThai = N'Đang bán'"; 
                    
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ProductsList.Add(new Product
                                {
                                    Id = reader.GetInt32(0),
                                    Name = reader.GetString(1),
                                    Price = reader.IsDBNull(2) ? 0 : reader.GetDecimal(2),
                                    ImageData = reader.IsDBNull(3) ? null : (byte[])reader["HinhAnh"],
                                    Category = reader.IsDBNull(4) ? "Khác" : reader.GetString(4),
                                    
                                    TonKho = reader.IsDBNull(5) ? 0 : reader.GetInt32(5) 
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kết nối CSDL: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            icProducts.ItemsSource = FilteredProducts;
            if (icCartItems != null) icCartItems.ItemsSource = CartList;

            if (ProductsList.Count > 0)
            {
                var categories = ProductsList.Select(p => p.Category)
                                             .Where(c => !string.IsNullOrEmpty(c))
                                             .Distinct()
                                             .ToList();
                categories.Insert(0, "Tất cả"); 
                lstCategories.ItemsSource = categories;
                lstCategories.SelectedIndex = 0; 
            }
            ApplyFilters();
        }

        
        private void ApplyFilters()
        {
            if (ProductsList == null || FilteredProducts == null) return;

            string keyword = txtSearch.Text?.ToLower() ?? "";

            var filtered = ProductsList.Where(p => 
                (currentCategory == "Tất cả" || p.Category == currentCategory) &&
                (string.IsNullOrEmpty(keyword) || p.Name.ToLower().Contains(keyword))
            ).ToList();

            FilteredProducts.Clear();
            foreach (var item in filtered)
            {
                FilteredProducts.Add(item);
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters(); 
        }

        private void LstCategories_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstCategories.SelectedItem != null)
            {
                currentCategory = lstCategories.SelectedItem.ToString();
                ApplyFilters(); 
            }
        }


        private void BtnCart_Click(object sender, RoutedEventArgs e)
        {
            CartOverlay.Visibility = Visibility.Visible;
            UpdateCheckoutSummary();
        }

        private void BtnCloseCart_Click(object sender, RoutedEventArgs e)
        {
            CartOverlay.Visibility = Visibility.Collapsed;
        }

        private void BtnAddToCart_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            Product selectedProduct = btn.DataContext as Product;

            if (selectedProduct != null)
            {
                var existingItem = CartList.FirstOrDefault(c => c.Product.Id == selectedProduct.Id);
                
                int slDuKien = (existingItem != null) ? existingItem.Quantity + 1 : 1;

                if (slDuKien > selectedProduct.TonKho)
                {
                    MessageBox.Show($"Thuốc '{selectedProduct.Name}' hiện chỉ còn lại {selectedProduct.TonKho} sản phẩm.\nKhông thể mua thêm!", 
                                    "Hết hàng", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return; 
                }

                if (existingItem != null)
                {
                    existingItem.Quantity++;
                }
                else
                {
                    CartList.Add(new CartItem { Product = selectedProduct, Quantity = 1 });
                }
                UpdateCheckoutSummary();
            }
        }
        private void BtnIncreaseQty_Click(object sender, RoutedEventArgs e)
        {
            var cartItem = (sender as Button).DataContext as CartItem;
            if (cartItem != null)
            {
                if (cartItem.Quantity >= cartItem.Product.TonKho)
                {
                    MessageBox.Show($"Thuốc '{cartItem.Product.Name}' hiện chỉ còn lại {cartItem.Product.TonKho} sản phẩm.\nKhông thể tăng thêm!", 
                                    "Giới hạn tồn kho", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                cartItem.Quantity++;
                UpdateCheckoutSummary();
            }
        }

        private void BtnDecreaseQty_Click(object sender, RoutedEventArgs e)
        {
            var cartItem = (sender as Button).DataContext as CartItem;
            if (cartItem != null && cartItem.Quantity > 1)
            {
                cartItem.Quantity--;
                UpdateCheckoutSummary();
            }
        }

        private void BtnRemoveItem_Click(object sender, RoutedEventArgs e)
        {
            var cartItem = (sender as Button).DataContext as CartItem;
            if (cartItem != null)
            {
                CartList.Remove(cartItem);
                UpdateCheckoutSummary();
            }
        }

        private void UpdateCheckoutSummary()
        {
            txtCartCount.Text = CartList.Sum(c => c.Quantity).ToString();
            txtTotalItems.Text = txtCartCount.Text;
            decimal total = CartList.Sum(c => c.Product.Price * c.Quantity);
            txtTotalPrice.Text = total.ToString("N0") + " đ";
        }

        private void BtnCheckout_Click(object sender, RoutedEventArgs e)
        {
            if (CartList.Count == 0)
            {
                MessageBox.Show("Giỏ hàng của bạn đang trống!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    using (SqlTransaction transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            int idTuCustomers = Pharmacy_Manage.GUI.AppSession.CurrentCustomerID;
                            if (idTuCustomers == 0)
                            {
                                MessageBox.Show("Lỗi: Không tìm thấy thông tin đăng nhập của bạn!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }

                            int maKH_ThucSu = 0;

                            string getCust = "SELECT FullName, Phone, Email FROM Customers WHERE CustomerID = @id";
                            using (SqlCommand cmdGet = new SqlCommand(getCust, conn, transaction))
                            {
                                cmdGet.Parameters.AddWithValue("@id", idTuCustomers);
                                using (SqlDataReader reader = cmdGet.ExecuteReader())
                                {
                                    if (reader.Read())
                                    {
                                        string ten = reader["FullName"].ToString();
                                        string sdt = reader["Phone"].ToString();
                                        string email = reader["Email"].ToString();
                                        reader.Close();

                                        string checkKH = "SELECT MaKH FROM KhachHang WHERE SoDienThoai = @sdt OR Email = @email";
                                        using (SqlCommand cmdCheck = new SqlCommand(checkKH, conn, transaction))
                                        {
                                            cmdCheck.Parameters.AddWithValue("@sdt", sdt);
                                            cmdCheck.Parameters.AddWithValue("@email", email);
                                            object kq = cmdCheck.ExecuteScalar();
                                            
                                            if (kq != null && kq != DBNull.Value)
                                            {
                                                maKH_ThucSu = Convert.ToInt32(kq);
                                            }
                                            else
                                            {
                                                string insKH = "INSERT INTO KhachHang(HoTen, SoDienThoai, Email) VALUES(@ten, @sdt, @email); SELECT SCOPE_IDENTITY();";
                                                using (SqlCommand cmdIns = new SqlCommand(insKH, conn, transaction))
                                                {
                                                    cmdIns.Parameters.AddWithValue("@ten", ten);
                                                    cmdIns.Parameters.AddWithValue("@sdt", sdt);
                                                    cmdIns.Parameters.AddWithValue("@email", email);
                                                    maKH_ThucSu = Convert.ToInt32(cmdIns.ExecuteScalar());
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        reader.Close();
                                        throw new Exception("Không tìm thấy dữ liệu Customer.");
                                    }
                                }
                            }

                            decimal tongTienSP = CartList.Sum(c => c.Product.Price * c.Quantity);

                            string queryHoaDon = @"INSERT INTO HoaDon (MaKH, TongTienSanPham, TrangThai) 
                                                   OUTPUT INSERTED.MaHD 
                                                   VALUES (@MaKH, @TongTien, N'Đã thanh toán')";
                            
                            int maHD = 0;
                            using (SqlCommand cmdHD = new SqlCommand(queryHoaDon, conn, transaction))
                            {
                                cmdHD.Parameters.AddWithValue("@MaKH", maKH_ThucSu);
                                cmdHD.Parameters.AddWithValue("@TongTien", tongTienSP);
                                maHD = (int)cmdHD.ExecuteScalar();
                            }

                            string checkStockQuery = "SELECT TonKho FROM SanPham WHERE MaSP = @MaSP";
                            string queryCTHD = "INSERT INTO ChiTietHoaDon (MaHD, MaSP, SoLuong, DonGia) VALUES (@MaHD, @MaSP, @SoLuong, @DonGia)";
                            string queryUpdateKho = "UPDATE SanPham SET TonKho = TonKho - @SoLuong, HangXuat = HangXuat + @SoLuong WHERE MaSP = @MaSP";

                            foreach (var item in CartList)
                            {
                                using (SqlCommand cmdCheckStock = new SqlCommand(checkStockQuery, conn, transaction))
                                {
                                    cmdCheckStock.Parameters.AddWithValue("@MaSP", item.Product.Id);
                                    int tonKhoHienTai = Convert.ToInt32(cmdCheckStock.ExecuteScalar());

                                    if (item.Quantity > tonKhoHienTai)
                                    {
                                        throw new Exception($"Sản phẩm '{item.Product.Name}' không đủ tồn kho (hiện chỉ còn {tonKhoHienTai}).\n\nVui lòng giảm số lượng mua hoặc quay lại sau khi cửa hàng nhập thêm hàng nhé!");
                                    }
                                }

                                using (SqlCommand cmdCT = new SqlCommand(queryCTHD, conn, transaction))
                                {
                                    cmdCT.Parameters.AddWithValue("@MaHD", maHD);
                                    cmdCT.Parameters.AddWithValue("@MaSP", item.Product.Id);
                                    cmdCT.Parameters.AddWithValue("@SoLuong", item.Quantity);
                                    cmdCT.Parameters.AddWithValue("@DonGia", item.Product.Price);
                                    cmdCT.ExecuteNonQuery();
                                }

                              
                                using (SqlCommand cmdUpdate = new SqlCommand(queryUpdateKho, conn, transaction))
                                {
                                    cmdUpdate.Parameters.AddWithValue("@SoLuong", item.Quantity);
                                    cmdUpdate.Parameters.AddWithValue("@MaSP", item.Product.Id);
                                    cmdUpdate.ExecuteNonQuery();
                                }
                            }

                            
                            int diemCong = (int)(tongTienSP / 1000);
                            string queryCongDiem = "UPDATE Customers SET Points = ISNULL(Points, 0) + @DiemCong WHERE CustomerID = @IDCust"; 
                            using (SqlCommand cmdDiem = new SqlCommand(queryCongDiem, conn, transaction))
                            {
                                cmdDiem.Parameters.AddWithValue("@DiemCong", diemCong);
                                cmdDiem.Parameters.AddWithValue("@IDCust", idTuCustomers);
                                cmdDiem.ExecuteNonQuery();
                            }

                            transaction.Commit();

                            MessageBox.Show("Thanh toán thành công! Hóa đơn đã được lưu vào hệ thống.", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                            CartList.Clear();
                            UpdateCheckoutSummary();
                            CartOverlay.Visibility = Visibility.Collapsed;
                            LoadData(); 
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            
                            if (ex.Message.Contains("chỉ còn lại"))
                            {
                                MessageBox.Show(ex.Message, "Sản phẩm không đủ", MessageBoxButton.OK, MessageBoxImage.Warning);
                            }
                            else
                            {
                                MessageBox.Show("Lỗi trong quá trình xử lý đơn hàng:\n" + ex.Message, "Lỗi thanh toán", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kết nối CSDL: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public byte[] ImageData { get; set; }
        public string Category { get; set; } 
        public int TonKho { get; set; }
    }

    public class CartItem : INotifyPropertyChanged
    {
        public Product Product { get; set; }

        private int _quantity;
        public int Quantity
        {
            get { return _quantity; }
            set
            {
                _quantity = value;
                OnPropertyChanged(nameof(Quantity));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ByteArrayToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is byte[] bytes && bytes.Length > 0)
            {
                try
                {
                    using (var ms = new MemoryStream(bytes))
                    {
                        var image = new BitmapImage();
                        image.BeginInit();
                        image.CacheOption = BitmapCacheOption.OnLoad;
                        image.StreamSource = ms;
                        image.EndInit();
                        image.Freeze(); 
                        return image;
                    }
                }
                catch
                {
                    return null;
                }
            }
            return null; 
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}