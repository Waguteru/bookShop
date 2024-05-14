using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace bookShop
{
    public partial class Orders : Form
    {

        DataBase dataBase = new DataBase();
      //  private readonly CheckUser _user;

        private int pricebook;
        private string namebook;
        private string author;


        private string login_user;
        private string role;

        public Orders(string namebook,string author,int pricebook, Npgsql.NpgsqlConnection npgsqlConnection)
        {
            InitializeComponent();
            this.namebook = namebook;
            this.author = author;
            this.pricebook = pricebook;
            this.pricebook = pricebook;
            conn = npgsqlConnection;

            richTextBox1.Text =
                                $"Название книги: {this.namebook}\n\n" +
                                $"Автор: {this.author}\n\n" +
            //   $"количество: {}\n\n" +
                                $"Цена: {pricebook:C}\n\n" +
                                 $"Почта: {textBox2.Text:C}\n\n" +
                                $"Пункт выдачи: {comboBox1.Text:C}\n\n";

            numericUpDown1.Value = 1;
        }

        private void UpdateRichTextBox()
        {
            int quantity = (int)numericUpDown1.Value;

            if (quantity <= 0)
            {
                MessageBox.Show("Заказ не может быть пустым.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                FormUser formoOpen = new FormUser(login_user);
                formoOpen.Show();
                this.Close();
            }
            else
            {
                int totalAmount = (this.pricebook * quantity);

                // Обновляем содержимое richTextBox1 на основе сохраненных данных
                richTextBox1.Text =
                                $"Название книги: {this.namebook}\n\n" +
                                $"Автор: {this.author}\n\n" +
                                $"Количество: {quantity}\n\n" +
                                $"Начальная цена: {pricebook:C}\n\n" +
                                $"Сумма: {totalAmount:C}\n\n" +
                                $"Почта: {textBox2.Text:C}\n\n" +
                                $"ФИО: {textBox3.Text:C}\n\n" +
                                $"Пункт выдачи: {comboBox1.Text:C}\n\n";


                textBox1.Text = totalAmount.ToString();
            }
        }
        private NpgsqlConnection conn;

        private void Orders_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Получаем значения из richTextBox1
            string namebook = this.namebook;
            string author = this.author;
            int price_book = this.pricebook;
            int summa_order = int.Parse(textBox1.Text); // Общая сумма заказа
            int quantity = (int)numericUpDown1.Value;
            string email_user = textBox2.Text;
            string fio = textBox3.Text;
            string statusorders = "Неподтверждён";
            string commentorders = "-";

            // Сохраняем данные в базу данных
            using (NpgsqlCommand cmd = new NpgsqlCommand())
            {
                conn.Open();
                cmd.Connection = conn;

                // Запрос для вставки данных в таблицу orderuser
                string sql = "INSERT INTO orders (namebooks,author,price_book,summa_order,quantity,email_user,fio,statusorders,commentorders) " +
                             "VALUES (@namebooks, @author, @price_book, @summa_order, @quantity,@email_user,@fio,@statusorders,@commentorders)";
                cmd.CommandText = sql;
                // Параметры запроса
             
                cmd.Parameters.AddWithValue("@namebooks", namebook); // Предполагается, что название товара берется из поля name
                cmd.Parameters.AddWithValue("@author", author);
                cmd.Parameters.AddWithValue("@price_book", price_book);
                cmd.Parameters.AddWithValue("@summa_order", summa_order);
                cmd.Parameters.AddWithValue("@quantity", quantity);
                cmd.Parameters.AddWithValue("@email_user", email_user);
                cmd.Parameters.AddWithValue("@fio", fio);
                cmd.Parameters.AddWithValue("@statusorders", statusorders);
                cmd.Parameters.AddWithValue("@commentorders", commentorders);

                // Выполняем запрос
                cmd.ExecuteNonQuery();

                MessageBox.Show("Заказ успешно оформлен и сохранен в базе данных.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);

                FormUser formoOpen = new FormUser(login_user);
                this.Hide();
                formoOpen.ShowDialog();
                this.Close();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            UpdateRichTextBox();
        }
    }
}
