using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using System.Reflection.Emit;

namespace bookShop
{
    public partial class AdminForm : Form
    {

        DataBase dataBase = new DataBase();

        int selectedRow;

        enum RowState
        {
            Exited,
            New,
            Modifided,
            ModifidedNew,
            Deleted
        }

        public AdminForm(string login_user)
        {
            InitializeComponent();

            label5.Text = login_user;
            //label6.Text = role;
        }


        public void CreateColumns()
        {
            dataGridView1.Columns.Add("id_orders", "Номер заказа"); //0
            dataGridView1.Columns.Add("namebooks", "Название книги"); //1
            dataGridView1.Columns.Add("author", "Автор"); //2
            dataGridView1.Columns.Add("statusorders", "Статус заказа"); //3
            dataGridView1.Columns.Add("commentorders", "Комментарий"); //4
            dataGridView1.Columns.Add("email_user", "Почта");         //5
            dataGridView1.Columns.Add("fio", "ФИО");                //6
            dataGridView1.Columns.Add("summa_order", "Сумма заказа"); //7
            dataGridView1.Columns.Add("price_book", "Цена"); //8
            dataGridView1.Columns.Add("quantity", "Количество"); //9
            dataGridView1.Columns.Add("IsNew",String.Empty);
            dataGridView1.Columns["isNew"].Visible = false;
        }

        public void ReadSingleRows(DataGridView gridView,IDataRecord record)
        {
            gridView.Rows.Add(record.GetInt64(0),record.GetString(1),record.GetString(2),record.GetString(3),record.GetString(4),record.GetString(5),record.GetString(6),record.GetInt32(7),record.GetInt32(8),record.GetInt32(9), RowState.Modifided);
        }

        public void RefreshData(DataGridView gridView)
        {
            gridView.Rows.Clear();

            dataBase.OpenConnection();

            string query = "select * from orders";

            NpgsqlCommand command = new NpgsqlCommand(query,dataBase.GetConnection());

            NpgsqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                ReadSingleRows(gridView,reader);
            }
            reader.Close();
        }

        private void AdminForm_Load(object sender, EventArgs e)
        {
            CreateColumns();
            RefreshData(dataGridView1);
            textBox2.Visible = false;
            textBox3.Visible = false;
            textBox4.Visible = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string smtpServer = "smtp.mail.ru";
            int smtpPort = 587;

            string smtpUsername = "waguteru@mail.ru";
            string smtpPassword = "kG6K9KvM5PtpENRLi1Vp";

            using (SmtpClient smtpClient = new SmtpClient(smtpServer, smtpPort))
            {
                smtpClient.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                smtpClient.EnableSsl = true;

                using (MailMessage mailMessage = new MailMessage())
                {
                    mailMessage.From = new MailAddress(smtpUsername);
                    mailMessage.To.Add(textBox2.Text);
                    mailMessage.Subject = "Заказ в Книжном магазине";
                    mailMessage.Body =  $"Комментарий к заказу: {textBox1.Text}\n\n" +
                                      $"Состав заказа: {textBox4.Text}";

                    try
                    {
                        dataBase.OpenConnection();

                        var status = comboBox1.SelectedItem.ToString();
                        //  var executortb = comboBox2.SelectedItem.ToString();
                        var id = Convert.ToInt32(textBox3.Text);
                        var comment_colunm = textBox1.Text;

                        string query = $"UPDATE orders SET statusorders = '{status}',commentorders = '{comment_colunm}' WHERE id_orders = " + id;
                        NpgsqlCommand command = new NpgsqlCommand(@query, dataBase.GetConnection());
                        command.ExecuteNonQuery();
                        dataBase.CloseConnection();

                        MessageBox.Show("Данные успешно изменены");

                        RefreshData(dataGridView1);




                        smtpClient.Send(mailMessage);
                        MessageBox.Show("Сообщение отправлено");
                       
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Сообщение не отправлено {ex.Message}");
                    }
                }
            }
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            selectedRow = e.RowIndex;

            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dataGridView1.Rows[selectedRow];

                comboBox1.Text = row.Cells[3].Value.ToString();
               textBox1.Text = row.Cells[4].Value.ToString();
                textBox2.Text = row.Cells[5].Value.ToString();
                textBox3.Text = row.Cells[0].Value.ToString();
                textBox4.Text = row.Cells[1].Value.ToString();
            }
        }

        public void Search(DataGridView gridView)
        {
            gridView.Rows.Clear();

            string query = $"select * from orders where concat (fio,namebooks) like '%" + search_tb.Text + "%'";

            NpgsqlCommand comm = new NpgsqlCommand(query, dataBase.GetConnection());

            dataBase.OpenConnection();

            NpgsqlDataReader read = comm.ExecuteReader();

            while (read.Read())
            {
                ReadSingleRows(gridView, read);
            }

            read.Close();
        }

        private void search_tb_TextChanged(object sender, EventArgs e)
        {
            Search(dataGridView1);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            AddBooks books = new AddBooks();
            this.Hide();
            books.ShowDialog();
            this.Close();
        }
    }
}
