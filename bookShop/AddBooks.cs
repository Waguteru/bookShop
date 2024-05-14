using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace bookShop
{
    public partial class AddBooks : Form
    {
        DataBase dataBase = new DataBase();

        private string login_user;
        private string role;

        public AddBooks()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            dataBase.OpenConnection();

            var namebook = namebook_tb.Text;
            var author = author_tb.Text;
            var photo = photo_tb.Text;
            var price = Convert.ToInt32(numericUpDown1.Value);

            string query = $"insert into books (name_book,author,photobook,pricebook) values ('{namebook}', '{author}', '{photo}', '{price}')";

            NpgsqlCommand command = new NpgsqlCommand(query,dataBase.GetConnection());

            command.ExecuteNonQuery();

            MessageBox.Show("данные успешно добавлены!", "Успех!", MessageBoxButtons.OK, MessageBoxIcon.Information);

            AdminForm admin = new AdminForm(login_user);
            this.Hide();
            admin.ShowDialog();
            this.Close();

            dataBase.CloseConnection();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
                photo_tb.Text = openFileDialog1.FileName;
        }
    }
}
