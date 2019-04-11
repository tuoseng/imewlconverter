﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Studyzy.IMEWLConverter.Entities;
using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter.Generaters
{
    public class PinyinGenerater :BaseCodeGenerater, IWordCodeGenerater
    {
        private static Dictionary<string, List<string>> mutiPinYinWord;

        #region IWordCodeGenerater Members

    

        public override void GetCodeOfWordLibrary(WordLibrary wl)
        {
            if (wl.CodeType == CodeType.Pinyin)
            {
                return;
            }
            if (wl.CodeType == CodeType.TerraPinyin) //要去掉音调
            {
                for (int i = 0; i < wl.Codes.Count; i++)
                {
                    var row = wl.Codes[i];
                    for (int j = 0; j < row.Count; j++)
                    {
                        string s = row[j];
                        string py = s.Remove(s.Length - 1); //remove tone
                        wl.Codes[i][j] = py;
                    }
                }
                return;
            }
            //不是拼音，就调用GetCode生成拼音
            var code= GetCodeOfString(wl.Word);
            wl.Codes = code;
            wl.CodeType=CodeType.Pinyin;
        }

        /// <summary>
        ///     获得一个词的拼音
        ///     如果这个词不包含多音字，那么直接使用其拼音
        ///     如果包含多音字，则找对应的注音词，根据注音词进行注音
        ///     没有找到注音词的，使用默认拼音
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public override Code GetCodeOfString(string str)
        {
            if (IsInWordPinYin(str))
            {
                List<string> pyList = GenerateMutiWordPinYin(str);
                for (int i = 0; i < str.Length; i++)
                {
                    if (pyList[i] == null)
                    {
                        pyList[i] = PinyinHelper.GetDefaultPinyin(str[i]);
                    }
                }
                return new Code(pyList,true);
            }
            try
            {
                return new Code( PinyinHelper.GetDefaultPinyin(str),true);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return null;
            }
        }

        public virtual IList<string> GetAllCodesOfChar(char str)
        {
            return PinyinHelper.GetPinYinOfChar(str);
        }

        /// <summary>
        ///     因为使用了注音的方式，所以避免了多音字，一个词也只有一个音
        /// </summary>
        public virtual bool Is1CharMutiCode
        {
            get { return false; }
        }

        public virtual bool Is1Char1Code
        {
            get { return true; }
        }

        #endregion

        private void InitMutiPinYinWord()
        {
            if (mutiPinYinWord == null)
            {
                var wlList = new Dictionary<string, List<string>>();
                string[] lines = GetMutiPinyin().Split(new[] {"\r\n"}, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i];

                    string py = line.Split(' ')[0];
                    string word = line.Split(' ')[1];

                    var pinyin = new List<string>(py.Split(new[] {'\''}, StringSplitOptions.RemoveEmptyEntries));
                    wlList.Add(word, pinyin);
                }
                mutiPinYinWord = wlList;
            }
        }

        private string GetMutiPinyin()
        {
            //string path = ConstantString.PinyinLibPath;
            var sb = new StringBuilder();
            //if (File.Exists(path))
            //{
            //    string txt = FileOperationHelper.ReadFile(path);

            //    var reg = new Regex(@"^('[a-z]+)+\s[\u4E00-\u9FA5]+$");
            //    string[] lines = txt.Split(new[] {"\r\n"}, StringSplitOptions.RemoveEmptyEntries);
            //    for (int i = 0; i < lines.Length; i++)
            //    {
            //        if (reg.IsMatch(lines[i]))
            //        {
            //            sb.Append(lines[i] + "\r\n");
            //        }
            //    }
            //}
            sb.Append(Helpers.DictionaryHelper.GetResourceContent("WordPinyin.txt"));
            return sb.ToString();
        }

        /// <summary>
        ///     一个词中是否有多音字注音
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        private bool IsInWordPinYin(string word)
        {
            InitMutiPinYinWord();
            foreach (string key in mutiPinYinWord.Keys)
            {
                if (word.Contains(key))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        ///     产生一个词中多音字的拼音,没有的就空着
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        private List<string> GenerateMutiWordPinYin(string word)
        {
            InitMutiPinYinWord();
            var pinyin = new string[word.Length];
            foreach (string key in mutiPinYinWord.Keys)
            {
                if (word.Contains(key))
                {
                    int index = word.IndexOf(key);
                    for (int i = 0; i < mutiPinYinWord[key].Count; i++)
                    {
                        pinyin[index + i] = mutiPinYinWord[key][i];
                    }
                }
            }
            return new List<string>(pinyin);
        }
    }
}