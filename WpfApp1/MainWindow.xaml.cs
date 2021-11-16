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
        public MainWindow()
        {
            InitializeComponent();
        }

        // Create Cipher for encrypting and decrypting
        private Aes CreateCipher()
        {
            Aes cipher = Aes.Create();

            // The ISO10126 padding string consists of random data before the length.
            cipher.Padding = PaddingMode.ISO10126;

            // Key has to be 32-bit so use Hash of password
            byte[] passwordBytes = Encoding.UTF8.GetBytes(PasswordText.Text);
            byte[] aesKey = SHA256Managed.Create().ComputeHash(passwordBytes);
            cipher.Key = aesKey;

            // Generate IV 
            cipher.GenerateIV();

            // Debug
            Trace.WriteLine("Key: " + Convert.ToBase64String(aesKey) + " IV: " + Convert.ToBase64String(cipher.IV));

            return cipher;
        }

        // Show error if no text is entered
        private bool IsTextEmpty()
        {
            if (InputText.Text == "")
            {
                OutputText.Text = "No input text!";
                return true;
            }
            else return false;
        }

        // Encrypt
        private void EncryptButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsTextEmpty()) return;

            try
            {
                // Get cipher
                Aes cipher = CreateCipher();

                // Create encryption
                ICryptoTransform cryptotransform = cipher.CreateEncryptor(cipher.Key, cipher.IV);
                byte[] plaintext = Encoding.UTF8.GetBytes(InputText.Text);
                byte[] ciphertext = cryptotransform.TransformFinalBlock(plaintext, 0, plaintext.Length);

                // Append IV to encrypted cipher text to use when decrypting
                byte[] encryptedBytesAndIV = new byte[ciphertext.Length + cipher.IV.Length];
                cipher.IV.CopyTo(encryptedBytesAndIV, 0);
                ciphertext.CopyTo(encryptedBytesAndIV, cipher.IV.Length);

                // Show encrypted message
                OutputText.Text = Convert.ToBase64String(encryptedBytesAndIV);
            }
            catch (Exception ex) // Error handling
            {
                OutputText.Text = "<<< ERROR! catch : " + ex.ToString();
            }
        }

        // Decrypt
        private void DecryptButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsTextEmpty()) return;

            try
            {
                // Get cipher
                Aes cipher = CreateCipher();
                
                // Convert text from Base64 to array
                byte[] inputText = Convert.FromBase64String(InputText.Text);

                // Extract text and IV from string
                var iv = new byte[16];
                Array.Copy(inputText, 0, iv, 0, iv.Length);

                byte[] decryptText = new byte[inputText.Length - 16];
                Buffer.BlockCopy(inputText, 16, decryptText, 0, decryptText.Length);

                // Create decryption
                ICryptoTransform cryptoTransform = cipher.CreateDecryptor(cipher.Key, iv);
                byte[] cipherText = cryptoTransform.TransformFinalBlock(decryptText, 0, decryptText.Length);

                // Show decrypted message
                OutputText.Text = Encoding.UTF8.GetString(cipherText);
            }
            catch (Exception ex) // Error handling
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

        // Drag window
        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        // Close program
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // Copy text
        private void CopyTextButton_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(OutputText.Text);
        }

        // Paste text
        private void PasteTextButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                InputText.Text = Clipboard.GetText();
            }
            catch (Exception ex) // Error handling
            {
                OutputText.Text = "<<< ERROR! catch : " + ex.ToString();
            }
        }
    }
}