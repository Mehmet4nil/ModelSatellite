using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AForge.Video;
using AForge.Video.DirectShow;
using Excel = Microsoft.Office.Interop.Excel;


namespace v2_otomasyon_xbee_bmp_gps_kamera
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        Bitmap originalImage, originalImage3;
        string data;
        string[] veriler;
        int i, j;
        private FilterInfoCollection videoDevices;
        private VideoCaptureDevice videoSource;
        private void Form1_Load(object sender, EventArgs e)
        {
            originalImage = new Bitmap(pictureBox2.Image);
            originalImage3 = new Bitmap(pictureBox3.Image);
            Control.CheckForIllegalCrossThreadCalls = false;
            dataGridView1.ColumnCount = 16;
            dataGridView1.RowCount = 10000;
            dataGridView1.Columns[0].Name = "TAKIM NO";
            dataGridView1.Columns[1].Name = "PAKET NO";
            dataGridView1.Columns[2].Name = "GÖNDERME ZAMANI";
            dataGridView1.Columns[3].Name = "BASINÇ";
            dataGridView1.Columns[4].Name = "YÜKSEKLİK";
            dataGridView1.Columns[5].Name = "İNİŞ HIZI";
            dataGridView1.Columns[6].Name = "SICAKLIK";
            dataGridView1.Columns[7].Name = "PİL GERİLİMİ";
            dataGridView1.Columns[8].Name = "GPS LATITUDE";
            dataGridView1.Columns[9].Name = "GPS LONGITUDE";
            dataGridView1.Columns[10].Name = "GPS ALTITUDE";
            dataGridView1.Columns[11].Name = "UYDU STATÜSÜ";
            dataGridView1.Columns[12].Name = "PITCH";
            dataGridView1.Columns[13].Name = "ROLL";
            dataGridView1.Columns[14].Name = "YAW";
            dataGridView1.Columns[15].Name = "DONÜŞ SAYISI";
            timer1.Start();

            Array ports = SerialPort.GetPortNames();
            for (int k = 0; k < ports.Length; k++)
            {
                cmbPort.Items.Add(ports.GetValue(k));
            }
            
            cmbBaundRate.Items.Add("300");
            cmbBaundRate.Items.Add("1200");
            cmbBaundRate.Items.Add("2400");
            cmbBaundRate.Items.Add("4800");
            cmbBaundRate.Items.Add("9600");
            cmbBaundRate.Items.Add("19200");
            cmbBaundRate.Items.Add("38400");
            cmbBaundRate.Items.Add("57600");
            cmbBaundRate.Items.Add("74880");
            cmbBaundRate.Items.Add("115200");
            cmbBaundRate.Items.Add("230400");
            cmbBaundRate.Items.Add("250000");
            cmbBaundRate.SelectedIndex = 4;
            videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach (FilterInfo device in videoDevices)
            {
                cmbDevice.Items.Add(device.Name);
            }
            videoSource = new VideoCaptureDevice();
            //veriler[0] = "a";
            //for (int i = 0; i < 19; i++)
            //{
            //    veriler[i] = "q";
            //}
        }
        void videoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            Bitmap image = (Bitmap)eventArgs.Frame.Clone();
            pictureBox1.Image = image;
        }
        private void btnKameraAc_Click(object sender, EventArgs e)
        {
            //serialPort1.Write("b");
            try
            {
                
                if (videoSource.IsRunning)
                {
                    videoSource.Stop();
                    pictureBox1.Image = null;
                    pictureBox1.Invalidate();
                }
                else
                {
                    videoSource = new VideoCaptureDevice(videoDevices[cmbDevice.SelectedIndex].MonikerString);
                    videoSource.NewFrame += videoSource_NewFrame;
                    videoSource.Start();
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
            
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (videoSource.IsRunning)
            {
                videoSource.Stop();
            }
        }

        private void btnBaglantiAc_Click(object sender, EventArgs e)
        {
            try
            {
                serialPort1.BaudRate = Convert.ToInt32(cmbBaundRate.SelectedItem);
                serialPort1.PortName = Convert.ToString(cmbPort.SelectedItem);
                serialPort1.Open();
                
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void btnBaglantiKapat_Click(object sender, EventArgs e)
        {
            try
            {
                serialPort1.Close();
                timer1.Stop();
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }
        
        private void serialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                /*
                0-sayac
                1-sıcaklık
                2-basınc
                3-yukseklık
                4-satel
                5-lan
                6-long
                7-alt
                8-saat
                9-dakıka
                10-saniye
                11-voltaj
                */
                
                data = serialPort1.ReadLine();
                veriler = data.Split(',');
                dataGridView1.Rows[i].Cells[j].Value = "47801"; j++;
                dataGridView1.Rows[i].Cells[j].Value = veriler[0]; j++;
                ////dataGridView1.Rows[i].Cells[j].Value = (Convert.ToInt16(veriler[8])+3).ToString() +":"+ veriler[9] +":"+ veriler[10]; j++;
                ////dataGridView1.Rows[i].Cells[j].Value = veriler[8] +  veriler[9] +  veriler[10]; j++;
                //dataGridView1.Rows[i].Cells[j].Value = DateTime.Now.ToLongTimeString(); j++;
                dataGridView1.Rows[i].Cells[j].Value = veriler[1] + "/" + veriler[2] + "/" + veriler[3] + "," + veriler[4] + "/" + veriler[5] + "/" + veriler[6]; j++;
                dataGridView1.Rows[i].Cells[j].Value = veriler[7] + " Pa"; j++;
                dataGridView1.Rows[i].Cells[j].Value = veriler[8] + " m"; j++;
                dataGridView1.Rows[i].Cells[j].Value = veriler[9] + " m/s"; j++;
                dataGridView1.Rows[i].Cells[j].Value = veriler[10] + " Cᵒ"; j++;
                dataGridView1.Rows[i].Cells[j].Value = veriler[11] + " V"; j++;
                dataGridView1.Rows[i].Cells[j].Value = veriler[12] + " N"; j++;
                dataGridView1.Rows[i].Cells[j].Value = veriler[13] + " S"; j++;
                dataGridView1.Rows[i].Cells[j].Value = veriler[14] + " m"; j++;
                dataGridView1.Rows[i].Cells[j].Value = veriler[15]; j++;
                dataGridView1.Rows[i].Cells[j].Value = veriler[16] + " ᵒ"; j++;
                dataGridView1.Rows[i].Cells[j].Value = veriler[17] + " ᵒ"; j++;
                dataGridView1.Rows[i].Cells[j].Value = veriler[18] + " ᵒ"; j++;
                dataGridView1.Rows[i].Cells[j].Value = veriler[19]; j++;
                //dataGridView1.Rows[i].Cells[j].Value = veriler[8]; j++;
                pictureBox2.Image = RotateImage(originalImage, Int32.Parse(veriler[16]));
                pictureBox3.Image = RotateImage(originalImage3, Int32.Parse(veriler[17]));
                i++;
                j = 0;
                
            }
            catch (Exception ex)
            {

                //MessageBox.Show(ex.Message);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                this.chart1.Series["BASINÇ"].Points.AddXY(DateTime.Now.ToLongTimeString(), veriler[7]);
                this.chart2.Series["YÜKSEKLİK"].Points.AddXY(DateTime.Now.ToLongTimeString(), veriler[8]);
                this.chart3.Series["İNİŞ HIZI"].Points.AddXY(DateTime.Now.ToLongTimeString(), veriler[9]);
                this.chart4.Series["SICAKLIK"].Points.AddXY(DateTime.Now.ToLongTimeString(), veriler[10]);
                this.chart5.Series["PİL GERİLİMİ"].Points.AddXY(DateTime.Now.ToLongTimeString(), veriler[11]);
                
            }
            catch (Exception ex)
            {

                //MessageBox.Show(ex.Message);
            }
            
        }

       

        private void button1_Click(object sender, EventArgs e)
        {
           serialPort1.Write("A");
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            serialPort1.Write("A");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            serialPort1.Write("K");
        }

        private void veriKaydet_Click(object sender, EventArgs e)
        {
            copyAlltoClipboard();
            Microsoft.Office.Interop.Excel.Application xlexcel;
            Microsoft.Office.Interop.Excel.Workbook xlWorkBook;
            Microsoft.Office.Interop.Excel.Worksheet xlWorkSheet;
            object misValue = System.Reflection.Missing.Value;
            xlexcel = new Excel.Application();
            xlexcel.Visible = true;
            xlWorkBook = xlexcel.Workbooks.Add(misValue);
            xlWorkSheet = (Excel.Worksheet)xlWorkBook.Worksheets.get_Item(1);
            Excel.Range CR = (Excel.Range)xlWorkSheet.Cells[1, 1];
            CR.Select();
            xlWorkSheet.PasteSpecial(CR, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, true);
        }
        private void copyAlltoClipboard()
        {
            dataGridView1.SelectAll();
            DataObject dataObj = dataGridView1.GetClipboardContent();
            if (dataObj != null)
                Clipboard.SetDataObject(dataObj);
        }

        private void videoKaydet_Click(object sender, EventArgs e)
        {

        }

        private void btnGrafikCiz_Click(object sender, EventArgs e)
        {
            try
            {
                timer1.Start();
            }
            catch (Exception ex)
            {

                //MessageBox.Show(ex.Message);
            }
            
        }



        public static Image RotateImage(Image img, float rotationAngle)

        {
            //create an empty Bitmap image

            Bitmap bmp = new Bitmap(img.Width, img.Height);

            //turn the Bitmap into a Graphics object

            Graphics gfx = Graphics.FromImage(bmp);

            //now we set the rotation point to the center of our image

            gfx.TranslateTransform((float)bmp.Width / 2, (float)bmp.Height / 2);

            //now rotate the image

            gfx.RotateTransform(rotationAngle);

            gfx.TranslateTransform(-(float)bmp.Width / 2, -(float)bmp.Height / 2);

            //set the InterpolationMode to HighQualityBicubic so to ensure a high
            //quality image once it is transformed to the specified size

            gfx.InterpolationMode = InterpolationMode.HighQualityBicubic;

            //now draw our new image onto the graphics object

            gfx.DrawImage(img, new Point(0, 0));

          //dispose of our Graphics object

            gfx.Dispose();

            //return the image

            return bmp;

        }
    }
}
