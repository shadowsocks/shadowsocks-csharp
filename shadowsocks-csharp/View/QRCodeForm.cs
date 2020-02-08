﻿using Shadowsocks.Controller;
using Shadowsocks.Model;
using Shadowsocks.Properties;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ZXing.QrCode.Internal;

namespace Shadowsocks.View
{
    public partial class QRCodeForm : Form
    {
        private string code;

        public QRCodeForm(string code)
        {
            this.code = code;
            InitializeComponent();
            Icon = Icon.FromHandle(Resources.ssw128.GetHicon());
            Text = I18N.GetString("QRCode and URL");
        }

        private void GenQR(string ssconfig)
        {
            string qrText = ssconfig;
            QRCode code = ZXing.QrCode.Internal.Encoder.encode(qrText, ErrorCorrectionLevel.M);
            ByteMatrix m = code.Matrix;
            int blockSize = Math.Max(pictureBox1.Height / m.Height, 1);

            int qrWidth = m.Width * blockSize;
            int qrHeight = m.Height * blockSize;
            int dWidth = pictureBox1.Width - qrWidth;
            int dHeight = pictureBox1.Height - qrHeight;
            int maxD = Math.Max(dWidth, dHeight);
            pictureBox1.SizeMode = maxD >= 7 * blockSize ? PictureBoxSizeMode.Zoom : PictureBoxSizeMode.CenterImage;

            Bitmap drawArea = new Bitmap((m.Width * blockSize), (m.Height * blockSize));
            using (Graphics g = Graphics.FromImage(drawArea))
            {
                g.Clear(Color.White);
                using (Brush b = new SolidBrush(Color.Black))
                {
                    for (int row = 0; row < m.Width; row++)
                    {
                        for (int col = 0; col < m.Height; col++)
                        {
                            if (m[row, col] != 0)
                            {
                                g.FillRectangle(b, blockSize * row, blockSize * col, blockSize, blockSize);
                            }
                        }
                    }
                }
            }
            pictureBox1.Image = drawArea;
        }

        private void QRCodeForm_Load(object sender, EventArgs e)
        {
            Configuration servers = Configuration.Load();
            List<KeyValuePair<string, string>> serverDatas = servers.configs.Select(
                server =>
                    new KeyValuePair<string, string>(ShadowsocksController.GetServerURL(server), server.ToString())
                ).ToList();
            listBox1.DataSource = serverDatas;

            int selectIndex = serverDatas.FindIndex(serverData => serverData.Key.StartsWith(code));
            if (selectIndex >= 0)
            {
                listBox1.SetSelected(selectIndex, true);
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string url = (sender as ListBox)?.SelectedValue.ToString();
            GenQR(url);
            textBoxURL.Text = url;
        }

        private void textBoxURL_Click(object sender, EventArgs e)
        {
            textBoxURL.SelectAll();
        }
    }
}
