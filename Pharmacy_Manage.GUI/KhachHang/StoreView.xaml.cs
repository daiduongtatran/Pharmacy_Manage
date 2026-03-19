using Microsoft.Data.SqlClient;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Pharmacy_Manage.GUI.KhachHang
{
    public partial class StoreView : UserControl
    {
        private string connectionString = @"Data Source=localhost;Initial Catalog=PharmacyManage;Integrated Security=True;TrustServerCertificate=True";
        public ObservableCollection<Product> ProductsList { get; set; }
        public ObservableCollection<Product> FilteredProducts { get; set; }
        public ObservableCollection<CartItem> CartList { get; set; }

        public StoreView()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    
                    // Lấy dữ liệu từ bảng SanPham và chỉ hiển thị thuốc đang bán
                    string query = "SELECT MaSP, TenSP, GiaBan FROM SanPham WHERE TrangThai = N'Đang bán'"; 
                    
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ProductsList.Add(new Product
                                {
                                    Id = reader.GetInt32(0),    // Cột MaSP
                                    Name = reader.GetString(1),  // Cột TenSP
                                    Price = reader.IsDBNull(2) ? 0 : reader.GetDecimal(2) // Cột GiaBan
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
        }


        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (FilteredProducts == null || ProductsList == null) return;
            string keyword = txtSearch.Text.ToLower();
            FilteredProducts.Clear();
            foreach (var item in ProductsList.Where(p => p.Name.ToLower().Contains(keyword)))
            {
                FilteredProducts.Add(item);
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

            MessageBox.Show("Thanh toán thành công! Cảm ơn bạn đã mua sắm tại MediTrack.", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            
            CartList.Clear();
            UpdateCheckoutSummary();
            CartOverlay.Visibility = Visibility.Collapsed;
        }
    }

    // --- Các lớp Model nội bộ dùng cho cửa hàng ---
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
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
}
