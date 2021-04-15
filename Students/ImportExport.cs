using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.IO;
using System.Windows.Forms;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using OfficeOpenXml.Table;

namespace Students
{
    class ImportExport : StudentsApplication
    {
        public static void ExportMain()
        {
            if (Table.Rows.Count == 0)
            {
                MessageBox.Show(@"Nada que exportar.", @"Fallo la Exportacion", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = @"Text files (*.txt)|*.txt|Excel files (*.xlsx)|*.xlsx",
                FilterIndex = 2,
                RestoreDirectory = true,
                AddExtension = true,
                AutoUpgradeEnabled = true,
                CheckPathExists = true,
                OverwritePrompt = true,
            };

            if (saveFileDialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            FileStream stream = (FileStream)saveFileDialog.OpenFile();
            var extension = Path.GetExtension(stream.Name);

            //
            // Export to Excel
            //
            if(extension != ".xlsx" && extension != ".txt")
            {
                MessageBox.Show(@"Formato Incorrecto. Por favor usar los formatos .xlsx o .txt",
                                @"Formato del Archivo Incorrecto",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                stream.Close();
                return;
            }

            if (extension.Equals(".xlsx"))
            {
                ExcelPackage pck = new ExcelPackage(stream);
                var ws = pck.Workbook.Worksheets;

                //
                //hoja de estudiantes suscriptores
                //
                var enrolledStudentSheet = ws.Add("Enrolled Students");

                //estilo de las celdas
                enrolledStudentSheet.Cells["A1:E1"].Style.Font.Bold = true;

                //Cargar la tabla de estudiantes dentro de la tabla de excel
                enrolledStudentSheet.Cells["A1"].LoadFromDataTable(Table, true);

                //Crear Tabla de Excel
                var enrolledStudentRange = enrolledStudentSheet.Cells[1, 1, Table.Rows.Count + 1, Table.Columns.Count];
                var enrolledStudentExcelTable = enrolledStudentSheet.Tables.Add(enrolledStudentRange, "EnrolledStudents");

                //Estilo de la tabla de Excel
                enrolledStudentExcelTable.TableStyle = TableStyles.Light2;
                enrolledStudentExcelTable.ShowHeader = true;

                //Centrar el texto de las celdas.
                enrolledStudentSheet.Cells[1, 1, Table.Rows.Count + 1, Table.Columns.Count].Style.HorizontalAlignment = ExcelHorizontalAlignment.CenterContinuous;

                enrolledStudentSheet.DefaultColWidth = 20;
                enrolledStudentSheet.Cells.AutoFitColumns();

                //
                //hoja de estudiantes
                //
                var studentRecordSheet = ws.Add("Student Record");

                // estilo de las celdas
                studentRecordSheet.Cells["A1:F1"].Style.Font.Bold = true;

                //Cargar la tabla de estudiantes dentro de la tabla de excel
                var studentRecordTable = CollectionToDataTable(StudentRecord);
                studentRecordSheet.Cells["A1"].LoadFromDataTable(studentRecordTable, true);

                //Crear Tabla de Excel
                var studentRecordSheetRange = studentRecordSheet.Cells[1, 1, studentRecordTable.Rows.Count + 1, studentRecordTable.Columns.Count];
                var studentRecordExcelTable = studentRecordSheet.Tables.Add(studentRecordSheetRange, "StudentRecord");

                //Estilo de la Tabla
                studentRecordExcelTable.TableStyle = TableStyles.Light2;

                //Centrar el texto de las celdas.
                studentRecordSheet.Cells[1, 1, studentRecordTable.Rows.Count + 1, studentRecordTable.Columns.Count].Style.HorizontalAlignment = ExcelHorizontalAlignment.CenterContinuous;

                studentRecordSheet.DefaultColWidth = 20;
                studentRecordSheet.Cells.AutoFitColumns();


                //Save & dispose
                pck.Save();
                pck.Dispose();

                enrolledStudentSheet.Dispose();
                studentRecordTable.Dispose();
            }

            //
            //Export to text file
            //
            else if (extension.Equals(".txt"))
            {
                var enrolledStudentsTable = Table;

                StreamWriter writer = new StreamWriter(stream);
                writer.Write(Environment.NewLine);

                //
                //Write enrolled stdents table
                //

                //Table name
                writer.WriteLine(enrolledStudentsTable.TableName);
                writer.WriteLine();

                //Initialize the columns
                foreach(DataColumn col in enrolledStudentsTable.Columns)
                {
                    writer.Write($@"    {col.ColumnName}  |");
                }
                writer.WriteLine();

                //Start writing the rows
                foreach (DataRow row in enrolledStudentsTable.Rows)
                {
                    object[] rowData = row.ItemArray;
                    int i;
                    for (i=0; i < rowData.Length -1; i++)
                    {
                        writer.Write($" {rowData[i]}    | ");
                    }
                    writer.WriteLine(rowData[i].ToString());
                }

                writer.Write($"********** END OF {enrolledStudentsTable.TableName} DATA **********");
                writer.WriteLine();

                //
                // Write student record table
                //
                writer.WriteLine();
                var studentRecordTable = CollectionToDataTable(StudentRecord);

                //Table name
                writer.WriteLine(studentRecordTable.TableName);
                writer.WriteLine();

                //Initialize the columns
                foreach(DataColumn col in studentRecordTable.Columns)
                {
                    writer.Write($@"    {col.ColumnName}    |");
                }
                writer.WriteLine();

                //Start writing the rows
                foreach (DataRow row in studentRecordTable.Rows)
                {
                    object[] rowData = row.ItemArray;
                    int i;
                    for (i = 0; i < rowData.Length - 1; i++)
                    {
                        writer.Write($"     {rowData[i]}    |");
                    }
                    writer.WriteLine($@"    {rowData[i]}    ");
                }
                
                writer.Write($"********** END OF {studentRecordTable.TableName} DATA **********");

                //Dispose
                writer.Close();
            }

            //Dispose
            stream.Close();
        }

        public static bool ImportMain()
        {
            var check = MessageBox.Show(@"Esta seguro que desea cargar este archivo?" + "\n" +
                @"Se eliminara la  tabla actual", @"Confirmar Import", MessageBoxButtons.OK, MessageBoxIcon.Question);
            
            if (check == DialogResult.No)
            {
                return false;
            }

            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = @"Text files (*.txt)|*.txt|Excel files (*.xlsx)|*.xlsx",
                FilterIndex = 2,
                RestoreDirectory = true,
                AddExtension = true,
                AutoUpgradeEnabled = true,
                CheckPathExists = true,
                CheckFileExists = true,
            };

            if (openFileDialog.ShowDialog() != DialogResult.OK)
            {
                return false;
            }

            var stream = File.OpenRead(openFileDialog.FileName);

            //
            //Import from Excel
            //

            var extension = stream.Name.Substring(stream.Name.LastIndexOf("."));

            //Simple file format validation
            if(extension != ".xlsx" && extension != ".txt")
            {
                MessageBox.Show(@"Formato Incorrecto. Usar solo los formatos .xlsx o .txt.", @"Formato Incorrecto",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);

                stream.Close();
                return false;
            }

            if (extension == ".xlsx")
            {
                var pck = new ExcelPackage(stream);

                var ws = pck.Workbook.Worksheets.First();

                DataTable table = new DataTable { TableName = ws.Name };

                foreach (var firstRow in ws.Cells[1, 1, 1, ws.Dimension.End.Column])
                {
                    table.Columns.Add(firstRow.Text);
                }

                for (int i = 1; i < ws.Dimension.End.Row; i++)
                {
                    var wsRow = ws.Cells[i + 1, 1, i, ws.Dimension.End.Column];
                    // No agregar registros que yengan vacios los campos;
                    //Primer Nombre
                    //Segundo Nombre
                    //Email

                    if(ws.Cells[i + 1, 1, i, ws.Dimension.End.Column-1].Any(x => x.Text == ""))
                    {
                        continue;
                    }

                    DataRow row = table.Rows.Add();
                    foreach(var cell in wsRow)
                    {
                        row[cell.Start.Column - 1] = cell.Text;
                    }
                }

                Table = table;

                return true;
            }
            else
            {
                StreamReader reader = new StreamReader(stream);
                DataTable table = new DataTable();
                
                while (true)
                {
                    var line = reader.ReadLine();

                    if (line == "")
                    {
                        continue;
                    }

                    if (line == $"********** END OF {table.TableName} DATA **********")
                    {
                        break;
                    }

                    if (char.IsLetter(line[0]))
                    {
                        //Asumimos que la linea guarda el nombre de la tabla
                        //porque es la forma en que previamente exportamos la info.
                        table.TableName = line.Trim();
                        continue;
                    }

                    var splited = line.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray();

                    if (splited[0].ToUpper().Equals("ID"))
                    {
                        foreach(string colName in splited)
                        {
                            table.Columns.Add(colName);
                        }
                        continue;
                    }
                    table.Rows.Add(splited);
                }
                Table = table;
                stream.Dispose();
                return true;
            }
        }

        private static DataTable CollectionToDataTable(List<Student> collection)
        {
            var table = new DataTable
            {
                TableName = "Student Record"
            };

            var props = typeof(Student).GetProperties().TakeWhile(x => x.Name != "AcceptButton");

            foreach (var prop in props)
            {
                table.Columns.Add(prop.Name, prop.PropertyType);
            }

            foreach (var student in collection)
            {
                var row = new object[]
                {
                    student.Id,
                    student.FirstName,
                    student.LastName,
                    student.FullName,
                    student.Email,
                    student.PhoneNumber
                };
                table.Rows.Add(row);
            }

            return table;
        }

    }

}
