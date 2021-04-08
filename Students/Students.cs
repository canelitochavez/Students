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

        private void buttonClearFields_Click(object sender, EventArgs e)
        {
            textFirstName.Text = string.Empty;
            textLastName.Text = string.Empty;
            textBoxEmail.Text = string.Empty;
            textBoxFhoneNumber.Text = string.Empty;
        }

        private void dataGridViewStudents_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            _indexRow = e.RowIndex;

            if(e.RowIndex == -1)
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

        }
    }
}
