﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Diagnostics;

namespace EnigmaStimulator
{
    class Enigma
    {
        //Enigma常量
        static readonly string[] rotor_match = new string[] { "EKMFLGDQVZNTOWYHXUSPAIBRCJ", "AJDKSIRUXBLHWTMCQGZNPYFVOE", "BDFHJLCPRTXVZNYEIWGAKMUSQO", "ESOVPZJAYQUIRHXLNFTGKDCMWB", "VZBRGITYUPSDNHLXAWMJQOFECK" };
        static readonly string[] rotor_match_verse = new string[] { "UWYGADFPVZBECKMTHXSLRINQOJ", "AJPCZWRLFBDKOTYUQGENHXMIVS", "TAGBPCSDQEUFVNZHYIXJWLRKOM", "HZWVARTNLGUPXQCEJMBSKDYOIF", "QCYLXWENFTZOSMVJUDKGIARPHB" };
        static readonly string rotor_trigger = "RFWKA";
        static readonly string reflector = "YRUHQSLDPXNGOKMIEBFZCWVJAT";

        //输入/输出变量
        string plugboard;
        public int?[] rotor_num { get; private set; } //采用nullable，为了适应需要暴力破解的情况，下同
        public char?[] ring_setting { get; private set; }
        public char?[] message_key { get; private set; }
        string input_md5;
        char?[] input_message_key;
        public string md5 { get; private set; }
        string plain_text;
        string cypher_text;

        public Enigma(char[] plugboard, int?[] rotor_num, char?[] ring_setting, char?[] message_key, string plain_text = "", string cypher_text = "", string md5 = "")
        {
            input_message_key = new char?[] { null, null, null };
            if (plugboard.Length != 26) //检验plugboard输入是否合法
                throw new FormatException("Illegal Plugboard");
            for (int i = 0; i < 26; i++)
            {
                if (plugboard[i] != 'A' + i && plugboard[plugboard[i] - 'A'] != 'A' + i)
                    throw new FormatException("Illegal Plugboard");
            }
            this.plugboard = new string(plugboard); //给plugboard赋值

            if (rotor_num.Length != 3) //检验rotor_num输入是否合法
                throw new FormatException("Illegal Rotor");
            for (int i = 0; i < 3; i++)
            {
                if (rotor_num[i] > 5 || rotor_num[i] < 0)
                    throw new FormatException("Illegal Rotor");
            }
            this.rotor_num = rotor_num;

            if (ring_setting.Length != 3)
                throw new FormatException("Illegal Ring Setting");
            foreach (char? rs in ring_setting)
            {
                if (rs != null && !char.IsLetter((char)rs))
                    throw new FormatException("Illegal Ring Setting");
            }
            this.ring_setting = ring_setting;

            if (message_key.Length != 3)
                throw new FormatException("Illegal Message Key");
            foreach (char? mk in message_key)
            {
                if (mk != null && !char.IsLetter((char)mk))
                    throw new FormatException("Illegal Message Key");
            }
            this.message_key = message_key;
            Array.Copy(message_key, input_message_key, 3);

            plain_text = plain_text.Trim();
            foreach (char ch in plain_text)
                if (!char.IsLetter(ch))
                    throw new FormatException("Illegal Plain Text");
            this.plain_text = plain_text;

            foreach (char ch in cypher_text)
                if (!char.IsLetter(ch)) throw new FormatException("Illegal Cypher Text");
            this.cypher_text = cypher_text;

            foreach (char ch in md5)
                if (!char.IsLetterOrDigit(ch)) throw new FormatException("Illegal MD5");
            this.md5 = this.input_md5 = md5;
        }

        public string Encrypt()
        {
            cypher_text = string.Empty; //清空密文
            //检查是否可以计算
            if (plain_text == "")
                throw new ArithmeticException("Please input plain text");
            for (int i = 0; i < 3; i++)
            {
                if (rotor_num[i] == null || ring_setting == null || message_key[i] == null)
                    throw new ArithmeticException("Please input complete rotor");
            }
            md5 = Calculate_MD5(plain_text); //计算md5

            StringBuilder sb = new StringBuilder();
            foreach (char ch in plain_text)
            {
                var result = Through_Enigma(ch);
                sb.Append(result);
            }
            cypher_text = sb.ToString();
            Array.Copy(input_message_key, message_key, 3); //调回message_key, 防止decrypt时候变来变去
            return cypher_text;
        }

        public string Decrpyt()
        {
            if (cypher_text == "")
                throw new ArithmeticException("Please input cypher text");
            if (md5 == "")
                throw new ArithmeticException("Please input MD5");
            plain_text = cypher_text;
            input_md5 = md5;
            if (Recursive_Decrypt(rotor_num, ring_setting, input_message_key, 0))
            {
                return cypher_text;
            }
            else
                throw new ArithmeticException("No Solution");
        }

        /// <summary>
        /// 以下为功能函数
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>

        private bool Recursive_Decrypt(int?[] rotor_guess, char?[] ring_setting_guess, char?[] message_key_guess, int count) //count为“从前往后数最后一个确定的位数”
        {
            //对Rotor
            for (int i = 0; i < 3; i++)
            {
                if (rotor_guess[i] != null)
                    count++;
                else
                {
                    for (int j = 1; j <= 5; j++)
                    {
                        rotor_guess[i] = j;
                        if (Recursive_Decrypt(rotor_guess, ring_setting_guess, message_key_guess, 0))
                            return true;
                    }
                    rotor_guess[i] = null;
                    return false;
                }
            }
            //对Ring_Setting
            for (int i = 0; i < 3; i++)
            {
                if (ring_setting_guess[i] != null)
                    count++;
                else
                {
                    for (char j = 'A'; j <= 'Z'; j = (char)(j + 1))
                    {
                        ring_setting_guess[i] = j;
                        if (Recursive_Decrypt(rotor_guess, ring_setting_guess, message_key_guess, 0))
                            return true;
                    }
                    ring_setting_guess[i] = null;
                    return false;
                }
            }
            //对Message_Key
            for (int i = 0; i < 3; i++)
            {
                if (message_key_guess[i] != null)
                    count++;
                else
                {
                    for (char j = 'A'; j <= 'Z'; j = (char)(j + 1))
                    {
                        message_key_guess[i] = j;
                        if (Recursive_Decrypt(rotor_guess, ring_setting_guess, message_key_guess, 0))
                            return true;
                    }
                    message_key_guess[i] = null;
                    return false;
                }
            }
            if (count == 9) //所有参数均已知（或猜到已知）
            {
                Array.Copy(rotor_guess, rotor_num, 3);
                Array.Copy(message_key_guess, message_key, 3);
                Array.Copy(ring_setting_guess, ring_setting, 3);
                if (Calculate_MD5(Encrypt()) == input_md5)
                {
                    return true;
                }
                else
                    return false;
            }
            return false;
        }

        private string Calculate_MD5(string input) //计算MD5
        {
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();
        }

        private char Get_Next_Character(char input) //获得下一个字符字母
        {
            char next;
            if (input > 'Z' || input < 'A')
                throw new FormatException("加密/解密过程出错：输入并非字母");
            next = (char)(input + 1);
            if (next > 'Z')
                next = (char)(next - 26);
            return next;
        }

        private void Adjust_MessageKey()
        {
            message_key[2] = Get_Next_Character((char)message_key[2]);
            if (message_key[1] == rotor_trigger[(int)rotor_num[1] - 1] - 1)
            {
                message_key[1] = Get_Next_Character((char)message_key[1]);
                message_key[0] = Get_Next_Character((char)message_key[0]);
            }
            if (message_key[2] == rotor_trigger[(int)rotor_num[2] - 1])
            {
                message_key[1] = Get_Next_Character((char)message_key[1]);
            }
        }

        private char Through_Plugboard(char input)
        {
            if (input > 'Z' || input < 'A')
                throw new FormatException("Code Error, plz contact author");
            return plugboard[input - 'A'];
        }

        private char Through_Single_Rotor(char input, int number) //number = 0,1,2,3,4
        {
            int delta = (int)message_key[number] - (int)ring_setting[number];
            input = (char)(input + delta);
            if (input > 'Z')
                input = (char)(input - 26);
            else if (input < 'A')
                input = (char)(input + 26);
            input = rotor_match[(int)rotor_num[number] - 1][input - 'A'];
            input = (char)(input - delta);
            if (input > 'Z')
                input = (char)(input - 26);
            else if (input < 'A')
                input = (char)(input + 26);
            return input;
        }

        private char Through_Single_Rotor_Verse(char input, int number) //number = 0,1,2,3,4
        {
            int delta = (int)message_key[number] - (int)ring_setting[number];
            input = (char)(input + delta);
            if (input > 'Z')
                input = (char)(input - 26);
            else if (input < 'A')
                input = (char)(input + 26);
            input = rotor_match_verse[(int)rotor_num[number] - 1][input - 'A'];
            input = (char)(input - delta);
            if (input > 'Z')
                input = (char)(input - 26);
            else if (input < 'A')
                input = (char)(input + 26);
            return input;
        }

        private char Through_Rotor(char input)
        {
            if (input > 'Z' || input < 'A')
                throw new FormatException("Code Error, plz contact author");

            Adjust_MessageKey();
            for (int i = 2; i >= 0; i--)
                input = Through_Single_Rotor(input, i);

            return input;
        }

        private char Through_Rotor_Verse(char input)
        {
            if (input > 'Z' || input < 'A')
                throw new FormatException("Code Error, plz contact author");

            for (int i = 0; i <= 2; i++)
                input = Through_Single_Rotor_Verse(input, i);

            return input;
        }

        private char Through_Reflector(char input)
        {
            if (input > 'Z' || input < 'A')
                throw new FormatException("Code Error, plz contact author");
            return reflector[input - 'A'];
        }

        private char Through_Enigma(char input)
        {
            if (input > 'Z' || input < 'A')
                throw new FormatException("Code Error, plz contact author");
            input = Through_Plugboard(input);
            input = Through_Rotor(input);
            input = Through_Reflector(input);
            input = Through_Rotor_Verse(input);
            input = Through_Plugboard(input);
            return input;
        }
    }
}
