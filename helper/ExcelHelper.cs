using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections;
using System.Data;
using System.IO;
using System.Windows.Forms;

namespace Utils.helper
{
    public class ExcelHelper
    {
        /// <summary>
        /// 将DataGridView表格中的数据导出至Excel。所见即所得。
        /// </summary>
        /// <param name="dgv"></param>
        public static void ExportExcelOfDGV(DataGridView dgv)
        {
            try
            {
                SaveFileDialog sfd = new SaveFileDialog();
                //设置文件类型 
                sfd.Filter = "Excel files(*.xls)|*.xls|All files(*.*)|*.*";
                //设置默认文件类型显示顺序 
                sfd.FilterIndex = 1;
                //保存对话框是否记忆上次打开的目录
                sfd.RestoreDirectory = true;
                //点了保存按钮进入
                string localFilePath = string.Empty;
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    localFilePath = sfd.FileName.ToString(); //获得文件路径
                }
                else
                {
                    return;
                }

                //创建一个工作簿
                IWorkbook workbook = new HSSFWorkbook();

                //创建一个 sheet 表
                ISheet sheet = workbook.CreateSheet("Sheet1");

                // 设置每列的列宽
                int colWidthIndex = 0;
                foreach (DataGridViewColumn col in dgv.Columns)
                {
                    if (col.Visible)
                    {
                        if (col.Name.Contains("序号"))
                        {
                            sheet.SetColumnWidth(colWidthIndex, 5 * 256); // 5个字符宽度
                        }
                        else
                        {
                            sheet.SetColumnWidth(colWidthIndex, 20 * 256); // 20个字符宽度
                        }
                        colWidthIndex++;
                    }
                    //else
                    //{
                    //    sheet.SetColumnWidth(colWidthIndex, 0 * 256); // 15个字符宽度
                    //}

                }

                //创建一行
                IRow rowH = sheet.CreateRow(0);

                //创建一个单元格
                ICell cell = null;

                //创建单元格样式
                ICellStyle cellStyle = workbook.CreateCellStyle();

                // 设置单元格颜色
                cellStyle.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.SkyBlue.Index;
                cellStyle.FillPattern = FillPattern.SolidForeground;

                //创建格式
                IDataFormat dataFormat = workbook.CreateDataFormat();

                //设置为文本格式，也可以为 text，即 dataFormat.GetFormat("text");
                cellStyle.DataFormat = dataFormat.GetFormat("@");


                int colIndex = 0;
                //设置列名
                foreach (DataGridViewColumn col in dgv.Columns)
                {
                    if (col.Visible)
                    {
                        //创建单元格并设置单元格内容
                        rowH.CreateCell(colIndex).SetCellValue(col.Name);

                        //设置单元格格式
                        rowH.Cells[colIndex].CellStyle = cellStyle;

                        colIndex++;
                    }
                }

                //写入数据
                int dataColIndex = 0;
                for (int i = 0; i < dgv.Rows.Count; i++)
                {
                    //跳过第一行，第一行为列名
                    IRow row = sheet.CreateRow(i + 1);
                    dataColIndex = 0;
                    for (int j = 0; j < dgv.Columns.Count; j++)
                    {

                        if (dgv.Rows[i].Cells[j].Visible)
                        {
                            cell = row.CreateCell(dataColIndex);
                            cell.SetCellValue(dgv.Rows[i].Cells[j].Value.ToString());
                            // 所有单元格都设置为同样的背景色
                            //cell.CellStyle = cellStyle;
                            dataColIndex++;
                        }
                    }
                }

                //设置新建文件路径及名称
                string savePath = localFilePath + "-" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".xls";

                //创建文件
                FileStream file = new FileStream(savePath, FileMode.CreateNew, FileAccess.Write);

                //创建一个 IO 流
                MemoryStream ms = new MemoryStream();

                //写入到流
                workbook.Write(ms);

                //转换为字节数组
                byte[] bytes = ms.ToArray();

                file.Write(bytes, 0, bytes.Length);
                file.Flush();

                //还可以调用下面的方法，把流输出到浏览器下载
                //OutputClient(bytes);

                //释放资源
                bytes = null;

                ms.Close();
                ms.Dispose();

                file.Close();
                file.Dispose();

                workbook.Close();
                sheet = null;
                workbook = null;
                MessageBox.Show("导出成功！");
            }
            catch (Exception e)
            {
                MessageBox.Show("导出数据异常！" + e.Message);
            }
        }

        /// <summary>
        /// 导入Excel，将其中的数据保存在DataTable中，便于后续数据处理
        /// </summary>
        /// <param name="ExcelIncludeHeader"></param> 若Excel中第一行是表头，不是有效数据，则该参数为True，若Excel中的第一行不是表头，直接是有效数据，则该参数为False
        /// <returns></returns>
        public static DataTable ImportExcel(bool ExcelIncludeHeader)
        {
            DataTable dt = new DataTable("Excel_Data");

            OpenFileDialog ofd = new OpenFileDialog();
            // 设置文件类型 
            ofd.Filter = "All files(*.*)|*.*";
            // 设置默认文件类型显示顺序 
            ofd.FilterIndex = 1;
            // 保存对话框是否记忆上次打开的目录
            ofd.RestoreDirectory = true;
            // 点了保存按钮进入
            string filePath = string.Empty;
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                filePath = ofd.FileName.ToString(); // 获得文件路径
            }
            else
            {
                return dt;
            }

            IWorkbook wk = null;
            string extension = Path.GetExtension(filePath);
            try
            {
                FileStream fs = File.OpenRead(filePath);
                if (extension.Equals(".xls"))
                {
                    // 把xls文件中的数据写入wk中
                    wk = new HSSFWorkbook(fs);
                }
                else
                {
                    // 把xlsx文件中的数据写入wk中
                    wk = new XSSFWorkbook(fs);
                }

                fs.Close();
                // 读取当前表数据
                ISheet sheet = wk.GetSheetAt(0);
                // 读取当前行数据
                IRow row = sheet.GetRow(0);
                // EXCEL有表头的情况下，缓存表头各列信息
                ArrayList al = new ArrayList();

                // LastRowNum 是当前表的总行数-1（注意）
                for (int i = 0; i <= sheet.LastRowNum; i++)
                {
                    row = sheet.GetRow(i);  // 读取当前行数据

                    if (row != null)
                    {
                        DataRow dr = dt.NewRow();

                        //LastCellNum 是当前行的总列数
                        for (int j = 0; j < row.LastCellNum; j++)
                        {
                            //读取该行的第j列数据
                            object value = GetCellValue(row.GetCell(j));

                            // 给dt添加列名称
                            if (i == 0)
                            {
                                // 若EXCEL有表头，则dt中的列就设置为EXCEL中的表头
                                if (ExcelIncludeHeader)
                                {
                                    al.Add(value.ToString());
                                    dt.Columns.Add(value.ToString(), typeof(String));
                                }
                                else // 若EXCEL中没有表头，则dt中的列就按序号设置 第0列，第1列等
                                {
                                    dt.Columns.Add(j.ToString(), typeof(String));
                                }
                            }

                            if (ExcelIncludeHeader)
                            {
                                if (i != 0)
                                {
                                    dr[al[j].ToString()] = (value == null ? "" : value.ToString());
                                }
                            }
                            else // 若EXCEL中没有表头，则dt中的列就按序号设置 第0列，第1列等
                            {
                                dr[j.ToString()] = (value == null ? "" : value.ToString());//通过索引赋值
                            }
                        }
                        // 若Excel中包含表头，则不保存表头，只保存数据
                        if (i == 0)
                        {
                            if (!ExcelIncludeHeader)
                            {
                                dt.Rows.Add(dr);
                            }
                        }
                        else
                        {
                            dt.Rows.Add(dr);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            return dt;
        }

        /// <summary>
        /// 获取cell的数据，并设置为对应的数据类型
        /// </summary>
        /// <param name="cell"></param>
        /// <returns></returns>
        public static object GetCellValue(ICell cell)
        {
            object value = null;
            try
            {
                if (cell.CellType != CellType.Blank)
                {
                    switch (cell.CellType)
                    {
                        case CellType.Numeric:
                            // Date comes here
                            if (DateUtil.IsCellDateFormatted(cell))
                            {
                                value = cell.DateCellValue;
                            }
                            else
                            {
                                // Numeric type
                                value = cell.NumericCellValue;
                            }
                            break;
                        case CellType.Boolean:
                            // Boolean type
                            value = cell.BooleanCellValue;
                            break;
                        case CellType.Formula:
                            value = cell.CellFormula;
                            break;
                        default:
                            // String type
                            value = cell.StringCellValue;
                            break;
                    }
                }
            }
            catch (Exception)
            {
                value = "";
            }

            return value;
        }
    }
}
