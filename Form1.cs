﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EnigmaStimulator
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private char[] Alter(char[] plugboard, TextBox textBox)
        {
            if (textBox.Text.Length == 0)
                return plugboard;
            if (textBox.Text.Length != 2)
                MessageBox.Show("Plugboard输入有误");

            char ch1 = textBox.Text[0];
            char ch2 = textBox.Text[1];

            if (plugboard[ch1 - 'A'] != ch1 || plugboard[ch2 - 'A'] != ch2) //已经被换过了
                MessageBox.Show("Plugboard输入有误");
            else
            {
                plugboard[ch1 - 'A'] = ch2;
                plugboard[ch2 - 'A'] = ch1;
            }
            return plugboard;
        }

        private char[] Get_Plugboard()
        {
            char[] plugboard = new char[26];
            for (int i = 0; i < 26; i++)
                plugboard[i] = (char)('A' + i);

            //交换
            foreach (TextBox textBox in Plgbd.Controls)
                Alter(plugboard, textBox);

            return plugboard;
        } //根据winform输入，获取plugboard
        private int?[] Get_Rotor()
        {
            int?[] rotor = new int?[3];
            if (Rotor1.Text == string.Empty)
                rotor[0] = null;
            else
                rotor[0] = int.Parse(Rotor1.Text);

            if (Rotor2.Text == string.Empty)
                rotor[1] = null;
            else
                rotor[1] = int.Parse(Rotor2.Text);

            if (Rotor3.Text == string.Empty)
                rotor[2] = null;
            else
                rotor[2] = int.Parse(Rotor3.Text);

            return rotor;
        }

        private char?[] Get_Ring_Setting()
        {
            char?[] ring_setting = new char?[3];
            if (RS1.Text == string.Empty)
                ring_setting[0] = null;
            else
                ring_setting[0] = RS1.Text[0];

            if (RS2.Text == string.Empty)
                ring_setting[1] = null;
            else
                ring_setting[1] = RS2.Text[0];

            if (RS3.Text == string.Empty)
                ring_setting[2] = null;
            else
                ring_setting[2] = RS3.Text[0];

            return ring_setting;
        }

        private char?[] Get_Message_Key()
        {
            char?[] message_key = new char?[3];
            if (MK1.Text == string.Empty)
                message_key[0] = null;
            else
                message_key[0] = MK1.Text[0];

            if (MK2.Text == string.Empty)
                message_key[1] = null;
            else
                message_key[1] = MK2.Text[0];

            if (MK3.Text == string.Empty)
                message_key[2] = null;
            else
                message_key[2] = MK3.Text[0];

            return message_key;
        }

        private void encrypt_btn_Click(object sender, EventArgs e)
        {
            var plugboard = Get_Plugboard();
            var rotor_num = Get_Rotor();
            var ring_setting = Get_Ring_Setting();
            var message_key = Get_Message_Key();
            string plain_text = PlainText.Text;
            string cypher_text = CypherText.Text;
            string md5 = MD5.Text;

            Enigma enigma = new Enigma(plugboard, rotor_num, ring_setting, message_key, plain_text, cypher_text, md5);
            string cyphered = enigma.Encrypt();
            CypherText.ForeColor = Color.DarkBlue;
            CypherText.Text = cyphered;
            MD5.ForeColor = Color.DarkBlue;
            MD5.Text = enigma.md5;
            //Enigma enigma = new Enigma()
        }

        private void decrypt_btn_Click(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// 以下为UI部分的函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void Plgbd_Leave(object sender, EventArgs e)
        {
            TextBox this_textBox = (TextBox)sender;
            if (this_textBox.Text.Length == 1 || this_textBox.Text.Length > 2) //如果字符数不规范
                this_textBox.ForeColor = Color.Red;
            else this_textBox.ForeColor = Color.DarkGreen;

            //检测是否出现重复交换
            if (this_textBox.Text.Length == 2)
            {
                foreach (TextBox textBox in Plgbd.Controls)
                {
                    if (textBox == this_textBox)
                        continue;
                    if (textBox.Text.Contains(this_textBox.Text[0]) || textBox.Text.Contains(this_textBox.Text[1]))
                        this_textBox.ForeColor = Color.Red;
                }
            }
        }

        private void Plgbd_KeyPress(object sender, KeyPressEventArgs e) //处理Plugboard的键盘输入，自动更正非法项
        {
            //TODO：按下Tab之后自动切换到下一行

            TextBox textBox = (TextBox)sender;
            if (e.KeyChar == '\b') //如果输入的是退格键，则放行
                return;

            if (textBox.Text.Length >= 2 && textBox.SelectedText == "") //如果已经满了，则不计入这次输入
                e.Handled = true;

            if (e.KeyChar >= 'A' && e.KeyChar <= 'Z') //自动将小写转换为大写、并舍弃非字母
                ; //不执行任何东西
            else if (e.KeyChar >= 'a' && e.KeyChar <= 'z')
            {
                e.KeyChar = (char)(e.KeyChar - 32);
            }
            else
                e.Handled = true; //中止这次输入

            if (textBox.Text.Length == 1)
                SendKeys.Send("{tab}");
        }

        private void Rotor_KeyPress(object sender, KeyPressEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (e.KeyChar == '\b') //如果输入的是退格键，则放行
                return;

            if (textBox.Text.Length >= 1 && textBox.SelectedText == "") //如果已经满了，则不计入这次输入
                e.Handled = true;

            if (e.KeyChar < '0' || e.KeyChar > '5') //舍弃不合法输入
                e.Handled = true;
            if (textBox.Text.Length == 0) //自动跳进
                SendKeys.Send("{tab}");
        }

        private void Rotor_Leave(object sender, EventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (textBox.Text.Length > 1) //如果字符数不规范
                textBox.ForeColor = Color.Red;
            else textBox.ForeColor = Color.DarkGreen;
        }

        private void MKRS_KeyPress(object sender, KeyPressEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (e.KeyChar == '\b') //如果输入的是退格键，则放行
                return;

            if (textBox.Text.Length >= 1 && textBox.SelectedText == "") //如果已经满了，则不计入这次输入
                e.Handled = true;

            if (e.KeyChar >= 'A' && e.KeyChar <= 'Z') //自动将小写转换为大写、并舍弃非字母
                ;//不执行任何语句
            else if (e.KeyChar >= 'a' && e.KeyChar <= 'z')
            {
                e.KeyChar = (char)(e.KeyChar - 32);
            }
            else
                e.Handled = true;
            if (textBox.Text.Length == 0) //自动跳进
                SendKeys.Send("{tab}");
        }

        private void MKRS_Leave(object sender, EventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (textBox.Text.Length > 1) //如果字符数不规范
                textBox.ForeColor = Color.Red;
            else textBox.ForeColor = Color.DarkGreen;
        }

        private void Text_KeyPress(object sender, KeyPressEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            ///TODO:判断Ctrl引入的内容是否合法，防止bug
            if (e.KeyChar == '\b' || (Control.ModifierKeys & Keys.Control) == Keys.Control) //如果输入的是退格键或按下了Ctrl，则放行
                return;

            if (e.KeyChar >= 'A' && e.KeyChar <= 'Z') //自动将小写转换为大写、并舍弃非字母
                ;//不执行任何语句
            else if (e.KeyChar >= 'a' && e.KeyChar <= 'z')
            {
                e.KeyChar = (char)(e.KeyChar - 32);
            }
            else
                e.Handled = true;
        }

        private void MD5_KeyPress(object sender, KeyPressEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (e.KeyChar == '\b') //如果输入的是退格键，则放行
                return;

            if (e.KeyChar >= 'A' && e.KeyChar <= 'Z' || char.IsNumber(e.KeyChar)) //自动将小写转换为大写、并舍弃非字母、非数字
                ;//不执行任何语句
            else if (e.KeyChar >= 'a' && e.KeyChar <= 'z')
            {
                e.KeyChar = (char)(e.KeyChar - 32);
            }
            else
                e.Handled = true;
        }

        private void Text_Leave(object sender, EventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            textBox.ForeColor = Color.DarkGreen;
        }

        private void MD5_Leave(object sender, EventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (textBox.Text.Length == 16 || textBox.Text.Length == 32)
                textBox.ForeColor = Color.DarkGreen;
            else
                textBox.ForeColor = Color.Red;
        }

        private void Textbox_Enter(object sender, EventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            textBox.SelectAll();
        }

        private void Textbox_MouseClick(object sender, MouseEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            textBox.SelectAll();
        }
    }
}