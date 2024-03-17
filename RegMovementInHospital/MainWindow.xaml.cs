using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RegMovementInHospital
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<string> occupiedPatientIds = new List<string>(); // Коллекция для хранения занятых ID пациентов
        private TextBlock draggingTextBlock;

        public MainWindow()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            using (var entity = new BigBoarsEntities())
            {
                HospitaliGrid.ItemsSource = entity.Hospitalization.ToList();
            }

            // Проверяем существующие данные поля "Отделение" и устанавливаем соответствующее место в Canvas
            foreach (Hospitalization hospitalization in HospitaliGrid.ItemsSource)
            {
                string departmentValue = hospitalization.Department;
                if (!string.IsNullOrEmpty(departmentValue))
                {
                    string departmentNumber = departmentValue.Substring(10, 3); // Извлекаем номер отделения из строки "Отделение XXX, Койка X"
                    char bedLetter = departmentValue.Last(); // Получаем букву койки

                    // Находим соответствующий TextBlock по номеру отделения и букве койки
                    TextBlock targetTextBlock = FindTextBlockByDepartment(departmentNumber, bedLetter);
                    if (targetTextBlock != null)
                    {
                        targetTextBlock.Text = hospitalization.PatientId.ToString();
                        targetTextBlock.Background = Brushes.Red;
                        occupiedPatientIds.Add(hospitalization.PatientId.ToString());
                    }
                }
            }

            CustomGridColumns();
        }

        private TextBlock FindTextBlockByDepartment(string departmentNumber, char bedLetter)
        {
            string textBlockName = $"Room{departmentNumber}{bedLetter}";

            TextBlock foundTextBlock = null;

            FindTextBlockInChildren(ReceptionCanvas, textBlockName, ref foundTextBlock);

            return foundTextBlock;
        }

        private void FindTextBlockInChildren(DependencyObject parent, string name, ref TextBlock foundTextBlock)
        {
            if (parent is Grid)
            {
                Grid grid = parent as Grid;
                foreach (var child in grid.Children)
                {
                    if (child is TextBlock)
                    {
                        TextBlock textBlock = child as TextBlock;
                        if (textBlock.Name == name)
                        {
                            foundTextBlock = textBlock;
                            return;
                        }
                    }
                    // Рекурсивный обход дочерних элементов
                    FindTextBlockInChildren(child as DependencyObject, name, ref foundTextBlock);
                }
            }
            else if (parent is Panel)
            {
                Panel panel = parent as Panel;
                foreach (var child in panel.Children)
                {
                    // Рекурсивный обход дочерних элементов
                    FindTextBlockInChildren(child as DependencyObject, name, ref foundTextBlock);
                }
            }
        }

        private void CustomGridColumns()
        {
            HospitaliGrid.Loaded += (sender, e) =>
            {
                if(HospitaliGrid.Columns.Count > 0) 
                {
                    HospitaliGrid.Columns[0].Header = "ID госп.";
                    HospitaliGrid.Columns[1].Header = "ID пациента";
                    HospitaliGrid.Columns[2].Visibility = Visibility.Collapsed;
                    HospitaliGrid.Columns[3].Header = "Назнач. дата";
                    HospitaliGrid.Columns[4].Header = "Цель госп.";
                    HospitaliGrid.Columns[5].Header = "Отделение";
                    HospitaliGrid.Columns[6].Header = "Условия";
                    HospitaliGrid.Columns[7].Header = "Сроки";
                    HospitaliGrid.Columns[8].Header = "Доп.";
                    HospitaliGrid.Columns[9].Visibility = Visibility.Collapsed;
                }
            };
        }

        private void HospitaliGrid_MouseEnter(object sender, MouseEventArgs e)
        {
            HospitaliGrid.Width = 900;
        }

        private void HospitaliGrid_MouseLeave(object sender, MouseEventArgs e)
        {
            HospitaliGrid.Width = 185;
        }

        private void TextBlocks_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            TextBlock clickedTextBlock = sender as TextBlock;

            if (clickedTextBlock != null)
            {
                Hospitalization selectedHospitalization = HospitaliGrid.SelectedItem as Hospitalization;

                if (selectedHospitalization != null)
                {
                    if (occupiedPatientIds.Contains(selectedHospitalization.PatientId.ToString()))
                    {
                        MessageBox.Show("Этот пациент уже есть в палате!");
                    }
                    else
                    {
                        clickedTextBlock.Background = Brushes.Red;
                        clickedTextBlock.Text = selectedHospitalization.PatientId.ToString();
                        occupiedPatientIds.Add(selectedHospitalization.PatientId.ToString());

                        // Обновляем поле "Отделение" в базе данных
                        UpdateDepartment(selectedHospitalization, clickedTextBlock);
                    }
                }
            }
        }

        private void UpdateDepartment(Hospitalization hospitalization, TextBlock textBlock)
        {
            // Получаем номер отделения из имени TextBlock
            string departmentNumber = textBlock.Name.Substring(4, 3); // Получаем 3 цифры из имени TextBlock
            char bedLetter = textBlock.Name.Last(); // Получаем последнюю букву из имени TextBlock

            // Формируем новое значение для поля "Отделение"
            string departmentValue = $"Отделение {departmentNumber}, Койка {bedLetter}";

            // Обновляем значение в базе данных
            using (var entity = new BigBoarsEntities())
            {
                Hospitalization selectedHospitalization = entity.Hospitalization.FirstOrDefault(h => h.PatientId.ToString() == hospitalization.PatientId.ToString());
                if (selectedHospitalization != null)
                {
                    selectedHospitalization.Department = departmentValue;
                    entity.SaveChanges();
                    HospitaliGrid.ItemsSource = entity.Hospitalization.ToList(); // Обновляем DataGrid
                }
            }
        }

        private void TextBlock_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            draggingTextBlock = sender as TextBlock;
            if (draggingTextBlock != null && draggingTextBlock.Text != "")
            {
                DragDrop.DoDragDrop(draggingTextBlock, draggingTextBlock.Text, DragDropEffects.Move);
            }
        }

        private void TextBlock_Drop(object sender, DragEventArgs e)
        {
            TextBlock targetTextBlock = sender as TextBlock;
            if (targetTextBlock != null)
            {
                if (targetTextBlock.Text == "")
                {
                    targetTextBlock.Text = (string)e.Data.GetData(DataFormats.Text);
                    targetTextBlock.Background = Brushes.Red;
                    if (draggingTextBlock != null)
                    {
                        draggingTextBlock.Text = "";
                        draggingTextBlock.Background = Brushes.Transparent;
                    }
                    UpdateOccupancy(draggingTextBlock, targetTextBlock);
                }
            }
        }

        private void UpdateOccupancy(TextBlock source, TextBlock target)
        {
            if (source.Text != "")
            {
                occupiedPatientIds.Remove(source.Text);
            }
            if (target.Text != "")
            {
                // Получаем номер отделения из имени TextBlock
                string departmentNumber = target.Name.Substring(4, 3); // Получаем 3 цифры из имени TextBlock
                char bedLetter = target.Name.Last(); // Получаем последнюю букву из имени TextBlock

                // Формируем новое значение для поля "Отделение"
                string departmentValue = $"Отделение {departmentNumber}, Койка {bedLetter}";

                // Обновляем значение в базе данных
                using (var entity = new BigBoarsEntities())
                {
                    Hospitalization selectedHospitalization = entity.Hospitalization.FirstOrDefault(h => h.PatientId.ToString() == target.Text);
                    if (selectedHospitalization != null)
                    {
                        selectedHospitalization.Department = departmentValue;
                        entity.SaveChanges();
                        HospitaliGrid.ItemsSource = entity.Hospitalization.ToList(); // Обновляем DataGrid
                    }
                }

                occupiedPatientIds.Add(target.Text);
            }
        }

        private void TextBlock_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            TextBlock clickedTextBlock = sender as TextBlock;

            if (clickedTextBlock != null && !string.IsNullOrEmpty(clickedTextBlock.Text))
            {
                MessageBoxResult result = MessageBox.Show("Подтвердите выписку пациента", "Выписать пациента", MessageBoxButton.OKCancel);

                if (result == MessageBoxResult.OK)
                {
                    string patientId = clickedTextBlock.Text;

                    // Очищаем TextBlock
                    clickedTextBlock.Text = "";
                    clickedTextBlock.Background = Brushes.Transparent;

                    // Удаляем запись о пациенте из базы данных
                    using (var entity = new BigBoarsEntities())
                    {
                        Hospitalization selectedHospitalization = entity.Hospitalization.FirstOrDefault(h => h.PatientId.ToString() == patientId);
                        if (selectedHospitalization != null)
                        {
                            entity.Hospitalization.Remove(selectedHospitalization);
                            entity.SaveChanges();
                            HospitaliGrid.ItemsSource = entity.Hospitalization.ToList(); // Обновляем DataGrid
                        }
                    }

                    // Удаляем пациент из коллекции занятых ID
                    occupiedPatientIds.Remove(patientId);
                }
            }
        }
    }
}
