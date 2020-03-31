using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using System.Collections.ObjectModel;

using OpenFileDialog = System.Windows.Forms.OpenFileDialog;
using FolderBrowserDialog = System.Windows.Forms.FolderBrowserDialog;

namespace ImageScaler
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        List<string> selectedFileNames = new List<string>();
        List<BitmapImage> selectedImageSources = new List<BitmapImage>();
        ObservableCollection<Image> previewImages = new ObservableCollection<Image>();
        public MainWindow()
        {
            InitializeComponent();
            listview.ItemsSource = previewImages;
        }

        private void AddImageToScale(string filePath, string fileName)
        {
            if (selectedFileNames.Contains(fileName)) return;
            selectedFileNames.Add(fileName);
            BitmapImage source = new BitmapImage(new Uri(filePath, UriKind.RelativeOrAbsolute));
            selectedImageSources.Add(source);
            Image img = new Image();
            img.Source = source;
            img.Width = 200;
            previewImages.Add(img);
        }

        private void btnSelectImages_Click(object sender, RoutedEventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = "Images (*.jpg,*.png,*.bmp,*.gif)|*.jpg;*.png;*.bmp;*.gif";
                dialog.Multiselect = true;
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    for (int i = 0; i < dialog.FileNames.Length; i++)
                    {
                        string filePath = dialog.FileNames[i];
                        string fileName = dialog.SafeFileNames[i];
                        AddImageToScale(filePath, fileName);
                    }
                }
            }
        }

        private void btnScaleImage_Click(object sender, RoutedEventArgs e)
        {
            if (selectedFileNames.Count == 0)
            {
                MessageBox.Show("No image to resize!");
                return;
            }

            double.TryParse(scaleSizeInput.Text, out double newSize);
            if (newSize <= 0)
            {
                MessageBox.Show("New Size must be greater than zero");
                return;
            }

            using (FolderBrowserDialog saveDialog = new FolderBrowserDialog())
            {
                saveDialog.ShowNewFolderButton = true;
                saveDialog.Description = "Select location to output the images";
                if (saveDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string path = saveDialog.SelectedPath;
                    for (int i = 0; i < selectedImageSources.Count; i++)
                    {
                        BitmapImage image = selectedImageSources[i];
                        double width = image.PixelWidth;
                        double height = image.PixelHeight;
                        double ratio = width / height;

                        switch (scaleOptionInput.SelectedIndex)
                        {
                            case 0:
                                // Resize by width
                                width = newSize;
                                height = (int)(width / ratio);
                                break;
                            case 1:
                                // Resize by height
                                height = newSize;
                                width = (int)(height * ratio);
                                break;
                            case 2:
                                // Resize by scale up
                                width = (int)(width * newSize);
                                height = (int)(height * newSize);
                                break;
                            case 3:
                                // Resize by scale down
                                width = (int)(width / newSize);
                                height = (int)(height / newSize);
                                break;
                        }

                        string fileName = selectedFileNames[i];
                        TransformedBitmap bitmap = new TransformedBitmap(image, new ScaleTransform(width / image.PixelWidth, height / image.PixelHeight));

                        if (File.Exists(fileName)) File.Delete(fileName);
                        using (FileStream fs = File.Create(Path.Combine(path, fileName)))
                        {
                            PngBitmapEncoder encoder = new PngBitmapEncoder();
                            encoder.Frames.Add(BitmapFrame.Create(bitmap));
                            encoder.Save(fs);
                        }
                    }

                    MessageBox.Show("Resize completed!");
                }
            }
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            selectedFileNames.Clear();
            selectedImageSources.Clear();
            previewImages.Clear();
        }

        private void listview_Drop(object sender, System.Windows.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                for (int i = 0; i < files.Length; i++)
                {
                    string filePath = files[i];
                    string ext = Path.GetExtension(filePath);
                    switch (ext.ToLower())
                    {
                        case ".png":
                        case ".jpg":
                        case ".jpeg":
                        case ".bmp":
                        case ".gif":
                            AddImageToScale(filePath, Path.GetFileName(filePath));
                            break;
                    }
                }
            }
        }
    }
}
