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
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolTip;
using Image = System.Drawing.Image;

namespace bookShop
{
    public partial class AutenForm : Form
    {
        private bool closed = false;

        private string text = String.Empty;

        private int failedAttempts = 0;
        private DateTime blockedUntil = DateTime.MinValue;
      
        private bool captchaRequired = false;
        private string correctCaptcha = "";

        public AutenForm()
        {
            InitializeComponent();
        }

        /* private Bitmap CreateImage(int Width, int Height)
         {
             Random rnd = new Random();

             //Создадим изображение
             Bitmap result = new Bitmap(Width, Height);

             //Вычислим позицию текста
             int Xpos = rnd.Next(0, Width - 50);
             int Ypos = rnd.Next(15, Height - 15);

             //Добавим различные цвета
             Brush[] colors = { Brushes.Black,
                      Brushes.Red,
                      Brushes.RoyalBlue,
                      Brushes.Green };

             //Укажем где рисовать
             Graphics g = Graphics.FromImage((Image)result);

             //Пусть фон картинки будет серым
             g.Clear(Color.Gray);

             //Сгенерируем текст
             text = String.Empty;
             string ALF = "1234567890QWERTYUIOPASDFGHJKLZXCVBNM";
             for (int i = 0; i < 5; ++i)
                 text += ALF[rnd.Next(ALF.Length)];

             //Нарисуем сгенирируемый текст
             g.DrawString(text,
                          new Font("Arial", 15),
                          colors[rnd.Next(colors.Length)],
                          new PointF(Xpos, Ypos));

             //Добавим немного помех
             /////Линии из углов
             g.DrawLine(Pens.Black,
                        new Point(0, 0),
                        new Point(Width - 1, Height - 1));
             g.DrawLine(Pens.Black,
                        new Point(0, Height - 1),
                        new Point(Width - 1, 0));
             ////Белые точки
             for (int i = 0; i < Width; ++i)
                 for (int j = 0; j < Height; ++j)
                     if (rnd.Next() % 20 == 0)
                         result.SetPixel(i, j, Color.White);

             return result;
         }*/

        private string GenerateCaptcha()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            StringBuilder captcha = new StringBuilder();
            Random random = new Random();
            for (int i = 0; i < 6; i++)
            {
                captcha.Append(chars[random.Next(chars.Length)]);
            }
            return captcha.ToString();
        }


        private void button1_Click(object sender, EventArgs e)
        {
            string login_user = textBox1.Text;
            string password_user = textBox2.Text;

            if (closed)
            {
                return;
            }
            else if (DateTime.Now < blockedUntil)
            {
                MessageBox.Show($"Попробуйте ещё раз через {blockedUntil.Subtract(DateTime.Now).TotalSeconds} секунд");

                return;
            }
            else if (captchaRequired)
            {
                if (textBox3.Text != correctCaptcha)
                {
                    MessageBox.Show("превышено максимально количество попыток");

                    closed = true;
                    this.Close();
                    return;
                }

                failedAttempts = 0;
                captchaRequired = false;
                textBox3.Text = "";
               
                AdminForm form1 = new AdminForm(login_user);
                this.Hide();
                form1.ShowDialog();
                this.Close();
            }
            else if (CheckLogin(login_user,password_user))
            {
                ShowUserRoleForm(login_user);
            }
            else
            {
                failedAttempts++;
                if (failedAttempts == 3)
                {
                    blockedUntil = DateTime.Now.AddSeconds(10);
                    MessageBox.Show($"Вы ввели неправильно в 3 раз.Попробуйте через 30 сек");
                }
            }

            if (failedAttempts >= 4)
            {
                failedAttempts++;
                label4.Visible = true;
                textBox3.Visible = true;
                panel1.Visible = true;
                correctCaptcha = GenerateCaptcha();
                label4.Text = correctCaptcha;
                captchaRequired = true;
                MessageBox.Show($"Вы ввели в 4 раз неправильно.Код капчи: {correctCaptcha}");
            }

            if (failedAttempts >= 6)
            {
                closed = true;
                this.Close();
            }
        }

        string connectingString = "Server = localhost; port = 5432; Database = bookshop; User Id = postgres; Password = 123";

        private bool CheckLogin(string login_user, string password_user)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(connectingString))
            {
                string query = "SELECT COUNT(*) FROM users WHERE login_user = @login_user AND password_user = @password_user";
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("login_user", login_user);
                    command.Parameters.AddWithValue("password_user", password_user);

                    connection.Open();
                    int count = Convert.ToInt32(command.ExecuteScalar());
                    return count > 0;
                }
            }
        }

        private void ShowUserRoleForm(string login_user)
        {
            string role = GetUserRole(login_user);

            if (role == "Администратор")
            {
                AdminForm formApplication = new AdminForm(login_user);
                this.Hide();
                formApplication.ShowDialog();
                this.Close();
            }
            else if (role == "Пользователь")
            {
                FormUser formUser = new FormUser(login_user);
                this.Hide();
                formUser.ShowDialog();
                this.Close();
            }
            else
            {
                MessageBox.Show("ошибка: неизвестная роль пользователя", "ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            this.Hide();
        }

        private string GetUserRole(string login_user)
        {
            string role = "";
            using (NpgsqlConnection connection = new NpgsqlConnection(connectingString))
            {
                string query = "SELECT roles FROM users WHERE login_user = @login_user";
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@login_user", login_user);
                    try
                    {
                        connection.Open();
                        object result = command.ExecuteScalar();

                        if (result != null)
                        {
                            role = result.ToString();
                        }
                        else
                        {
                            MessageBox.Show("не удалось получить роль пользователя", "ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"ошибка при получении роли пользователя: {ex.Message}", "ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            return role;
        }

        private void AutenForm_Load(object sender, EventArgs e)
        {
            panel1.Visible = false;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            RegisterForm registerForm = new RegisterForm();
            this.Hide();
            registerForm.ShowDialog();
            this.Close();
        }
    }
}
