using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace bookShop
{
    public partial class FormUser : Form
    {

        DataBase dataBase = new DataBase();

        enum RowSate
        {
            New,
            Deleted,
            Modified,
            Modifiednew
        }


        public FormUser(string login_user)
        {
            InitializeComponent();
            button1.Visible = false;

            label3.Text = login_user;
          //  label4.Text = role;
        }


        private void FormUser_Load(object sender, EventArgs e)
        {
            CreateColumns();
            RefreshData(dataGridView1);
        }


        public void CreateColumns()
        {
            dataGridView1.Columns.Add("name_book", "Название книги");
            dataGridView1.Columns.Add("author", "Автор");
            dataGridView1.Columns.Add("pricebook", "Цена");
            dataGridView1.Columns.Add("photobook", "Изображение");
        }

        public void ReadSingleRows(DataGridView gridView, IDataRecord record)
        {
            gridView.Rows.Add(record.GetString(0), record.GetString(1), record.GetInt32(2), record.GetString(3), RowSate.Modified);
        }

        public void RefreshData(DataGridView gridView)
        {
            gridView.Rows.Clear();

            dataBase.OpenConnection();

            string query = "select name_book,author,pricebook,photobook from books";

            NpgsqlCommand command = new NpgsqlCommand(query, dataBase.GetConnection());

            NpgsqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                ReadSingleRows(gridView, reader);
            }
            reader.Close();
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];
                string imagePath = row.Cells[3].Value.ToString();

                if (!string.IsNullOrEmpty(imagePath))
                {

                    pictureBox1.ImageLocation = imagePath;
                    pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
                }
                else
                {
                    MessageBox.Show("Изображение не найдено");
                }
            }
        }

        public void Search(DataGridView gridView)
        {
            gridView.Rows.Clear();

            string query = $"select name_book,author,pricebook,photobook from books where concat (name_book,author) like '%" + textBox1.Text + "%'";

            NpgsqlCommand comm = new NpgsqlCommand(query, dataBase.GetConnection());

            dataBase.OpenConnection();

            NpgsqlDataReader read = comm.ExecuteReader();

            while (read.Read())
            {
                ReadSingleRows(gridView, read);
            }

            read.Close();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            Search(dataGridView1);
        }

        private int clickedRowIndex = -1;

        private void button1_Click(object sender, EventArgs e)
        {
            if (clickedRowIndex >= 0 && clickedRowIndex < dataGridView1.Rows.Count)
            {
                string namebook = dataGridView1.Rows[clickedRowIndex].Cells[0].Value.ToString();

                string author = dataGridView1.Rows[clickedRowIndex].Cells[1].Value.ToString();
                int pricebook = Convert.ToInt32(dataGridView1.Rows[clickedRowIndex].Cells[2].Value);



                NpgsqlConnection npgsqlConnection = new NpgsqlConnection("Server = localhost; port = 5432;Database = bookshop; User Id=postgres; Password = 123");

                Orders formOrders = new Orders(namebook, author, pricebook, npgsqlConnection);
                this.Hide();
                formOrders.ShowDialog();
                this.Close();

            }
        }

        private void dataGridView1_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right && e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                // Сохраняем индекс строки, на которую нажал пользователь правой кнопкой мыши
                clickedRowIndex = e.RowIndex;

                // Показываем panel1
                button1.Visible = true;

            }
        }
    }
}
