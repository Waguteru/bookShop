using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using Npgsql;

namespace bookShop
{
    public partial class RegisterForm : Form
    {

        DataBase dataBase = new DataBase();

        private string text = String.Empty;

        public RegisterForm()
        {
            InitializeComponent();
            panel1.Visible = false;

        }

       

        private static Regex PasswordValidation()
        {
            string pattern = "(?!^[0-9]*$)(?!^[a-zA-Z]*$)^([a-zA-Z0-9]{5,10})$";

            return new Regex(pattern, RegexOptions.IgnoreCase);
        }

        static Regex vaildate_password = PasswordValidation();

        private void button1_Click(object sender, EventArgs e)
        {
           
            if (vaildate_password.IsMatch(textBox2.Text) != true)
            {
                MessageBox.Show("Пароль должен состоять не менее чем из 5-10 символов. Он содержит не менее одного заглавноЙ буквы и цифр", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                textBox1.Focus();
                return;
            }
            else
            {
                dataBase.OpenConnection();

                var login = textBox1.Text;
                var password = textBox2.Text;
                var roles = "Пользователь";

                string query = $"insert into users (login_user,password_user,roles) values ('{login}', '{password}', '{roles}')";

                NpgsqlCommand command = new NpgsqlCommand(query,dataBase.GetConnection());

                command.ExecuteNonQuery();

                MessageBox.Show("Пароль верный");

                MessageBox.Show("данные успешно добавлены!", "Успех!", MessageBoxButtons.OK, MessageBoxIcon.Information);

                dataBase.CloseConnection();

                AutenForm autenForm = new AutenForm();
                this.Hide();
                autenForm.ShowDialog();
                this.Close();
            }
        }

        private void RegisterForm_Load(object sender, EventArgs e)
        {

        }
    }
}
