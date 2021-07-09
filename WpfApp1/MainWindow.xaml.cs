using System;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Security.Cryptography;
using System.Diagnostics;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Aes CreateCipher()
        {
            Aes cipher = Aes.Create();

            cipher.Padding = PaddingMode.ISO10126;

            byte[] passwordBytes = Encoding.UTF8.GetBytes(PasswordText.Text);
            byte[] aesKey = SHA256Managed.Create().ComputeHash(passwordBytes);
            cipher.Key = aesKey;

            cipher.GenerateIV();

            Trace.WriteLine("Key: " + Convert.ToBase64String(aesKey) + " IV: " + Convert.ToBase64String(cipher.IV));

            return cipher;
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void EncryptButton_Click(object sender, RoutedEventArgs e)
        {
            if (InputText.Text == "")
            {
                OutputText.Text = "No input text!";
                return;
            }

            try
            {
                Aes cipher = CreateCipher();

                ICryptoTransform cryptotransform = cipher.CreateEncryptor(cipher.Key, cipher.IV);
                byte[] plaintext = Encoding.UTF8.GetBytes(InputText.Text);
                byte[] ciphertext = cryptotransform.TransformFinalBlock(plaintext, 0, plaintext.Length);

                byte[] encryptedBytesAndIV = new byte[ciphertext.Length + cipher.IV.Length];
                cipher.IV.CopyTo(encryptedBytesAndIV, 0);
                ciphertext.CopyTo(encryptedBytesAndIV, cipher.IV.Length);

                OutputText.Text = Convert.ToBase64String(encryptedBytesAndIV);
            }
            catch (Exception ex)
            {
                OutputText.Text = "<<< ERROR! catch : " + ex.ToString();
            }
        }

        private void DecryptButton_Click(object sender, RoutedEventArgs e)
        {
            if (InputText.Text == "")
            {
                OutputText.Text = "No input text!";
                return;
            }

            try
            {
                Aes cipher = CreateCipher();
                
                byte[] plaintext = Convert.FromBase64String(InputText.Text);

                var iv = new byte[16];
                Array.Copy(plaintext, 0, iv, 0, iv.Length);

                byte[] decryptText = new byte[plaintext.Length - 16];
                Buffer.BlockCopy(plaintext, 16, decryptText, 0, decryptText.Length);

                ICryptoTransform cryptoTransform = cipher.CreateDecryptor(cipher.Key, iv);
                byte[] cipherText = cryptoTransform.TransformFinalBlock(decryptText, 0, decryptText.Length);

                OutputText.Text = Encoding.UTF8.GetString(cipherText);
            }
            catch (Exception ex)
            {
                if (ex is CryptographicException)
                {
                    OutputText.Text = "Invalid password!";
                    return;
                }
                else if (ex is ArgumentException || ex is FormatException) {
                    OutputText.Text = "Invalid input! \nInput must be Base64 do decrypt";
                    return;
                }
                OutputText.Text = "<<< ERROR! catch : " + ex.ToString();
            }
        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void CopyTextButton_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(OutputText.Text);
        }

        private void PasteTextButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                InputText.Text = Clipboard.GetText();
            }
            catch (Exception ex)
            {
                OutputText.Text = "<<< ERROR! catch : " + ex.ToString();
            }
        }
    }
}
