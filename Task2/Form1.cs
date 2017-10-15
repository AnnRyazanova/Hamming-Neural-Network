using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Task2
{
    public partial class Form1 : Form
    {
        private Bitmap img;
        private Bitmap binaryImg;
        private int [,] binaryMatrix;
        private Color background = Color.White;
        private double threshold = 50;
        private List<int[]> patternList;
        HammingNeuralNetwork hamming;

        public Form1()
        {
            InitializeComponent();
        }

        // Загрузка изображения + изменение формы в зависимости от размера картинки
        private void loadImg(Bitmap i, PictureBox p)
        {
            p.SizeMode = PictureBoxSizeMode.CenterImage;
           // Form1.ActiveForm.Height = i.Height + menuStrip1.Height + 50;
           // Form1.ActiveForm.Width = 2 * i.Width ;
            p.Image = i;
            p.Invalidate();
        }

        // Загрузка изображения (пользователь сам выбирает, из какой директории загружать картинку)
        private void loadImageToolStripMenuItem_Click(object sender, EventArgs e)
        {

            OpenFileDialog open_dialog = new OpenFileDialog();
            open_dialog.Filter = "Image Files(*.BMP;*.JPG;*.JPEG;*.GIF;*.PNG)|*.BMP;*.JPG;*.JPEG;*.GIF;*.PNG|All files (*.*)|*.*"; //формат загружаемого файла
            if (open_dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    img = new Bitmap(open_dialog.FileName);
                    loadImg(img, pictureBox1);
                }
                catch
                {
                    DialogResult rezult = MessageBox.Show("Невозможно открыть выбранный файл",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // Преобразует картинку из Bitmap в массив сигналов : 0 и 1
        private int[] transformPicture(Bitmap image)
        {
            int[] pix = new int[image.Size.Width * image.Size.Height];
            int index = 0;

            for (int i = 0; i < image.Size.Width; ++i)
                for (int j = 0; j < image.Size.Height; ++j)
                {
                    Color pixel = image.GetPixel(i, j);
                    double tmp = (pixel.R + pixel.G + pixel.B) / 3;

                    if ((Math.Max(pixel.R, background.R) - Math.Min(pixel.R, background.R) <= threshold) &&
                            (Math.Max(pixel.G, background.G) - Math.Min(pixel.G, background.G) <= threshold) &&
                            (Math.Max(pixel.B, background.B) - Math.Min(pixel.B, background.B) <= threshold))
                        pix[index] = 0;
                    else
                        pix[index] = 1;

                    index++;
                }
            return pix;
        }

        // Сохраняет бинарный сигнал в файл
        private void saveBinary(string [] s)
        {
            SaveFileDialog save_dialog = new SaveFileDialog();
            save_dialog.Filter = "Text Files(*.TXT)|*.TXT|All files (*.*)|*.*";
            if (save_dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    File.WriteAllLines(save_dialog.FileName, s);
                }
                catch
                {
                    DialogResult rezult = MessageBox.Show("Невозможно сохранить файл",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            }
        }

        // Бинаризация изображения
        private void binaryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // бинаризация цвета
            if (pictureBox1.Image != null)
            {

                int[] pix = transformPicture(img);
                int index = 0;
                string[] str = new string[img.Size.Width];

                for (int i = 0; i < img.Size.Width; ++i)
                {
                    for (int j = 0; j < img.Size.Height; ++j)
                    {
                        str[i] += pix[index].ToString();
                        index++;
                    }
                }
                saveBinary(str);
            }
            else
                MessageBox.Show("Изображение не загружено");
        }

        // Выбор заднего фона
        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            if (pictureBox1.Image != null)
                background = (img as Bitmap).GetPixel(e.Location.X, e.Location.Y);
            else
                MessageBox.Show("Изображение не загружено");
        }

        // Загрузка изображения из бинарного файла
        private void loadBinaryImgToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog open_dialog = new OpenFileDialog();
            open_dialog.Filter = "Text Files(*.TXT)|*.TXT|All files (*.*)|*.*"; 
            if (open_dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if (File.Exists(open_dialog.FileName))
                    {
                        var lines = File.ReadAllLines(open_dialog.FileName);

                        binaryImg = new Bitmap(lines.Length, lines[0].Length);
                        binaryMatrix = new int[lines.Length, lines[0].Length];

                        for (int x = 0; x < lines.Length; ++x)
                            for (int y = 0; y < lines[x].Length; ++y)
                            {
                                if (lines[x][y] == '0')
                                { 
                                    binaryImg.SetPixel(x, y, Color.White);
                                    binaryMatrix[x, y] = 0;
                                }
                                else if (lines[x][y] == '1')
                                { 
                                    binaryImg.SetPixel(x, y, Color.Black);
                                    binaryMatrix[x, y] = 1;
                                }
                                else
                                {
                                    MessageBox.Show("Что-то пошло не так... :(");
                                    return;
                                }
                            }

                        loadImg(binaryImg, pictureBox1);
                        
                    }
                    else
                        MessageBox.Show("Нет файла");
                }
                catch
                {
                    DialogResult res = MessageBox.Show("Невозможно открыть выбранный файл",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // Преобразование : от массива бинарных сигналов в Bitmap
        private Bitmap fromBinaryToBitmap(int [] binImage, int width, int height)
        {
            Bitmap res = new Bitmap(width, height);
            var i = 0;
            for (int x = 0; x < width; ++x)
                for (int y = 0; y < height; ++y)
                {
                    Color c = binImage[i++] == 0 ? Color.White : Color.Black;
                    res.SetPixel(x, y, c);
                 }

            return res;
        }

        // Результат работы нейронной сети : выдать образец, похожий на изображение от пользователя
        // Результат работы нейронной сети : выдать  номер образца, похожего на изображение от пользователя
        private void resultNetworkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
            {
                if (hamming != null)
                {
                    var imgForHamming = transformPicture(img);
                    var index = hamming.solution(imgForHamming);
                    if (index == -1)
                        MessageBox.Show("Невозможно определить принадлежность к классу");
                    else
                    {
                        MessageBox.Show("Изображение похоже на шаблон класса : " + index.ToString());

                        // получение изображения шаблона
                        var resVariant = patternList[index];
                        var resBitmap = fromBinaryToBitmap(resVariant, img.Width, img.Height);
                        pictureBox2.Image = resBitmap;
                    }
                }
                else
                    MessageBox.Show("Сначала сеть должна обучиться");
            }
            else
                MessageBox.Show("Загрузите изображение");
        }

        // Создание списка шаблонов + обучение сети
        private void learnNetworkСетиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fileDialog = new FolderBrowserDialog();

            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                var pathPatterns = fileDialog.SelectedPath;
                string[] files = Directory.GetFiles(pathPatterns);
                patternList = new List<int[]>();
                try
                {
                    foreach (var file in files)
                    {
                        var imgFromList = (Bitmap)Image.FromFile(file);
                        background = imgFromList.GetPixel(0, 0);
                        var binImg = transformPicture(imgFromList);
                        patternList.Add(binImg);
                    }
                    
                    hamming = new HammingNeuralNetwork(patternList);
                    MessageBox.Show("Обучение завершено");
                }
                catch (System.OutOfMemoryException)
                {
                    MessageBox.Show("В выбранной папке для обучения должны быть только изображения");
                }

            }
        }
    }
}
