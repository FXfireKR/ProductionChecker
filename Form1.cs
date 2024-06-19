using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace ProductChecker
{
    public partial class Form1 : Form
    {
        private const int DATA_HEADER_COUNT = 0;
        private const string DATA_FILE_PATH = "./predata.tmp";
        private const string OLD_DATA_FILE_PATH = "./cbj.mcx";

        public Form1()
        {
            InitializeComponent();
            this.FormClosed += Form1_FormClosed;

            dataGridView1.Columns.Add("ProductionName", "상품명");
            dataGridView1.Columns.Add("ProductionNumPerBox", "작업량");
            dataGridView1.Columns.Add("ProductionNeedNum", "발주요구량");
            dataGridView1.Columns.Add("ProductionNeedBoxNum", "필요상자");
            dataGridView1.Columns.Add("LeftedProduction", "남는물량");

            ReadRecentFile();
        }

        private void ReadRecentFile()
        {
            // 옛날 파일이 있다면 마이그레이션 해주자
            if (true == File.Exists(OLD_DATA_FILE_PATH) && false == File.Exists(DATA_FILE_PATH))
            {
                File.Copy(OLD_DATA_FILE_PATH, DATA_FILE_PATH);
                File.Delete(OLD_DATA_FILE_PATH);
            }

            // 파일을 자동으로 읽어오자
            if (true == File.Exists(DATA_FILE_PATH))
            {
                string strData = File.ReadAllText(DATA_FILE_PATH, Encoding.UTF8);
                string[] strRows = strData.Split(new char[] { '┌' });

                for (int i = 0; i < strRows.Length; ++i)
                {
                    string[] strCells = strRows[i].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    if (strCells.Length < 1) continue;

                    dataGridView1.Rows.Add(strCells);
                }
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            string strData = "";
            for (int i = DATA_HEADER_COUNT; i < dataGridView1.Rows.Count; ++i)
            {
                DataGridViewRow drv = dataGridView1.Rows[i];
                for (int j = 0; j < drv.Cells.Count; ++j)
                {
                    strData += drv.Cells[j].Value;
                    strData += ",";
                }
                strData += "┌";
            }


            File.WriteAllText(DATA_FILE_PATH, strData);
        }

        private void CaculateProductions()
        {
            try
            {
                for (int i = DATA_HEADER_COUNT; i < dataGridView1.Rows.Count; ++i)
                {
                    DataGridViewRow drv = dataGridView1.Rows[i];
                    bool bSkip = false;

                    for (int j = 0; j < drv.Cells.Count; ++j)
                    {
                        if (j == 1 || j == 2)
                        {
                            if (null == drv.Cells[j].Value)
                            {
                                bSkip = true;
                                break;
                            }
                        }
                    }

                    if (true == bSkip) continue;

                    string strProductName = drv.Cells[0].Value as string;
                    int iProductNumPerBox = 0;
                    if (false == Int32.TryParse(drv.Cells[1].Value as string, out iProductNumPerBox))
                    {
                        throw new Exception("작업량 값이 이상합니다.");
                    }

                    int iProductionNeedNum = 0;
                    if (false == Int32.TryParse(drv.Cells[2].Value as string, out iProductionNeedNum))
                    {
                        throw new Exception("발주 요구 값이 이상합니다.");
                    }

                    double needBox = (double)iProductionNeedNum / (double)iProductNumPerBox;
                    needBox = Math.Ceiling(needBox);

                    drv.Cells[3].Value = needBox;
                    drv.Cells[4].Value = ((int)needBox * iProductNumPerBox) - iProductionNeedNum;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "오류", MessageBoxButtons.OK);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (dataGridView1.Rows.Count > DATA_HEADER_COUNT)
            {   // 데이터가 있다.
                CaculateProductions();
            }
            else
            {
                MessageBox.Show("계산할 데이터를 입력 해주세요...", "오류", MessageBoxButtons.OK);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            dataGridView1.Rows.Clear();

            string[] strRows = Clipboard.GetText().Split(new char[] { '\n','\r' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string strRow in strRows)
            {
                string[] strCells = strRow.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (strCells.Length < 1) continue;

                dataGridView1.Rows.Add(strCells);
            }
        }
    }
}
