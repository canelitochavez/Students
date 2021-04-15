using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Mail;
using System.Windows.Forms;

namespace Students
{
    public partial class StudentsApplication : Form
    {
        public StudentsApplication()
        {
            InitializeComponent();
        }

        public static DataTable Table = new DataTable();
        private int _indexRow;
        protected static List<Student> StudentRecord = new List<Student>();

        private void StudentsApplication_Load(object sender, EventArgs e)
        {
            Table.TableName = "Enrolled Students";
            Table.Columns.Add("ID", typeof(int));
            Table.Columns.Add("First Name", typeof(string));
            Table.Columns.Add("Last Name", typeof(string));
            Table.Columns.Add("Email", typeof(string));
            Table.Columns.Add("Fhone Number", typeof(string));

            Table.PrimaryKey = new[] { Table.Columns["ID"] };

            dataGridViewStudents.DataSource = Table;
            dataGridViewStudents.AllowUserToAddRows = false;
            dataGridViewStudents.AllowUserToDeleteRows = false;
        }

        //
        //CREATE
        //
        private void buttonAddStudent_Click(object sender, EventArgs e)
        {
            if (!ValidateStudentInformation(textFirstName.Text,textLastName.Text,textBoxEmail.Text,textBoxFhoneNumber.Text))
            {
                return;
            }

            var student = new Student
            {
                FirstName = textFirstName.Text,
                LastName = textLastName.Text,
                FullName = textFirstName.Text + " " + textLastName.Text,
                Email = textBoxEmail.Text,
                PhoneNumber = textBoxFhoneNumber.Text == @" " ? "-" : textBoxFhoneNumber.Text
            };

            StudentRecord.Add(student);
            AddStudentToTable(student);

        }

        public void AddStudentToTable(Student student)
        {
            var row = Table.NewRow();

            row.ItemArray = new object[]
            {
                student.Id,
                student.FirstName,
                student.LastName,
                student.Email,
                student.PhoneNumber
            };

            Table.Rows.Add(row);
        }

        //
        //UPDATE
        //
        private void dataGridViewStudents_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            _indexRow = e.RowIndex;

            if (e.RowIndex == -1)
            {
                return;
            }

            DataGridViewRow row = dataGridViewStudents.Rows[_indexRow];

            textFirstName.Text = row.Cells["First Name"].Value.ToString();
            textLastName.Text = row.Cells["Last Name"].Value.ToString();
            textBoxEmail.Text = row.Cells["Email"].Value.ToString();
            textBoxFhoneNumber.Text = row.Cells["Fhone Number"].Value.ToString();
        }

        private void buttonUpdateStudent_Click(object sender, EventArgs e)
        {
            if(_indexRow == -1)
            {
                return;
            }

            if (!ValidateStudentInformation(textFirstName.Text, textLastName.Text, textBoxEmail.Text, textBoxFhoneNumber.Text))
            {
                return;
            }

            DataGridViewRow newDataRow = dataGridViewStudents.Rows[_indexRow];

            newDataRow.Cells["First Name"].Value = textFirstName.Text;
            newDataRow.Cells["Last Name"].Value = textLastName.Text;
            newDataRow.Cells["Email"].Value = textBoxEmail.Text;
            newDataRow.Cells["Fhone Number"].Value = textBoxFhoneNumber.Text; 

            var i = StudentRecord.FindIndex(x => x.Id == int.Parse(newDataRow.Cells["ID"].Value.ToString()));

            StudentRecord[i].FirstName = newDataRow.Cells["First Name"].Value.ToString();
            StudentRecord[i].LastName = newDataRow.Cells["Last Name"].Value.ToString();
            StudentRecord[i].Email = newDataRow.Cells["Email"].Value.ToString();
            StudentRecord[i].PhoneNumber = newDataRow.Cells["Fhone Number"].Value.ToString();
        }

        //
        //DELETE
        //
        private void buttonDeleteStudent_Click(object sender, EventArgs e)
        {
            DeleteStudent();
        }

        private void dataGridViewStudents_KeyUp(object sender, KeyEventArgs e)
        {
            if ((e.KeyCode & Keys.Delete) != Keys.Delete)
            {
                return;
            }

            DeleteStudent();
        }

        private void DeleteStudent()
        {
            var validate = MessageBox.Show(@"Esta seguro que desea Eliminar los Estudiantes seleccionados.",
                @"Confirmar Borrado.",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if(validate == DialogResult.No)
            {
                return;
            }

            var selected = dataGridViewStudents.SelectedRows;

            int i = 0;
            for (; i < selected.Count; i++)
            {
                DataGridViewRow row = selected[i];
                dataGridViewStudents.Rows.Remove(row);
            }

            MessageBox.Show($@"Se borraron {i} estudiantes.",
                @"Borrado Exitoso.",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void buttonClearFields_Click(object sender, EventArgs e)
        {
            textFirstName.Text = string.Empty;
            textLastName.Text = string.Empty;
            textBoxEmail.Text = string.Empty;
            textBoxFhoneNumber.Text = string.Empty;
        }


        //
        //VALIDATE INPUT
        //
        private bool ValidateStudentInformation(string firstName, string lastName, string email, string phoneNumber)
        {
            //Empty entry
            if (firstName == "" && lastName == "" && email == "")
            {
                MessageBox.Show(@"El Nombre completo del estudiante y su correo electronico son campos Obligatorios.",
                                @"Informacion del Estudiante Incorrecta.",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (firstName == "")
            {
                MessageBox.Show(@"El valor del campo Nombre no es valido.",
                                @"Campo Requerido.",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (lastName == "")
            {
                MessageBox.Show(@"El valor del campo Apellidos no es valido.",
                                @"Campo Requerido.",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (email == "")
            {
                MessageBox.Show(@"El valor del campo Email no es valido.",
                                @"Campo Requerido.",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            //invalid email format
            try
            {
                new MailAddress(email);
            }
            catch (Exception)
            {
                MessageBox.Show(@"El formato del campo Email no es valido.",
                 @"Formato del Correo Invalido.",
                 MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            //invalid phone number
            if(phoneNumber.Length != phoneNumber.Count(char.IsDigit) || phoneNumber.Length > 13)
            {
                MessageBox.Show(@"El valor del campo Numero de Telefono no es valido.",
                                @"Numero de Telefono Invalido.",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            //duplicate entry
            var rows = dataGridViewStudents.Rows;

            for(int i=0; i < rows.Count; i++)
            {
                if(rows[i].Cells["Email"].Value.ToString() != email || (rows[i].State & DataGridViewElementStates.Selected) != 0)
                {
                    continue;
                }
                else
                {
                    MessageBox.Show(@"El valor del campo Email ya existe, Intentar nuevamente con un nuevo Email.",
                                    @"Email Duplicado.",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }

            return true;
        }

        private void StudentsApplication_FormClosed(object sender, FormClosedEventArgs e)
        {
            Table.Dispose();
            dataGridViewStudents.Dispose();
        }

        private void buttonExport_Click(object sender, EventArgs e)
        {
            ImportExport.ExportMain();
        }

        private void buttonImport_Click(object sender, EventArgs e)
        {
            if (!ImportExport.ImportMain())
            {
                return;
            }

            dataGridViewStudents.Columns.Clear();
            dataGridViewStudents.DataSource = Table;
        }
    }
}
