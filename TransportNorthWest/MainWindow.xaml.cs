using Microsoft.Win32;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace TransportNorthWest
{
    public partial class MainWindow : Window
    {
        private TextBox[,] costBoxes;
        private TextBox[] supplyBoxes;
        private TextBox[] demandBoxes;

        private int[,] lastPlan;
        private int[,] lastCosts;
        private int[] lastSupply;
        private int[] lastDemand;
        private int lastTotalCost;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void CreateTables_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(SuppliersCountTextBox.Text, out int m) || m <= 0)
            {
                MessageBox.Show("Введите корректное количество поставщиков.");
                return;
            }

            if (!int.TryParse(ConsumersCountTextBox.Text, out int n) || n <= 0)
            {
                MessageBox.Show("Введите корректное количество потребителей.");
                return;
            }

            CreateInputTables(m, n);
        }

        private void CreateInputTables(int m, int n)
        {
            CostsGrid.Children.Clear();
            CostsGrid.RowDefinitions.Clear();
            CostsGrid.ColumnDefinitions.Clear();

            SupplyPanel.Children.Clear();
            DemandPanel.Children.Clear();

            costBoxes = new TextBox[m, n];
            supplyBoxes = new TextBox[m];
            demandBoxes = new TextBox[n];

            for (int i = 0; i <= m; i++)
                CostsGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            for (int j = 0; j <= n; j++)
                CostsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            for (int j = 1; j <= n; j++)
            {
                TextBlock header = new TextBlock
                {
                    Text = $"B{j}",
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(8),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetRow(header, 0);
                Grid.SetColumn(header, j);
                CostsGrid.Children.Add(header);
            }

            for (int i = 1; i <= m; i++)
            {
                TextBlock header = new TextBlock
                {
                    Text = $"A{i}",
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(8),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetRow(header, i);
                Grid.SetColumn(header, 0);
                CostsGrid.Children.Add(header);
            }

            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    TextBox tb = new TextBox
                    {
                        Width = 60,
                        Height = 34,
                        Margin = new Thickness(4),
                        HorizontalContentAlignment = HorizontalAlignment.Center,
                        VerticalContentAlignment = VerticalAlignment.Center
                    };

                    costBoxes[i, j] = tb;
                    Grid.SetRow(tb, i + 1);
                    Grid.SetColumn(tb, j + 1);
                    CostsGrid.Children.Add(tb);
                }
            }

            for (int i = 0; i < m; i++)
            {
                SupplyPanel.Children.Add(new TextBlock
                {
                    Text = $"Поставщик A{i + 1}",
                    FontWeight = FontWeights.SemiBold,
                    Margin = new Thickness(4, 8, 4, 2)
                });

                supplyBoxes[i] = new TextBox { Width = 120 };
                SupplyPanel.Children.Add(supplyBoxes[i]);
            }

            for (int j = 0; j < n; j++)
            {
                DemandPanel.Children.Add(new TextBlock
                {
                    Text = $"Потребитель B{j + 1}",
                    FontWeight = FontWeights.SemiBold,
                    Margin = new Thickness(4, 8, 4, 2)
                });

                demandBoxes[j] = new TextBox { Width = 120 };
                DemandPanel.Children.Add(demandBoxes[j]);
            }

            LogTextBox.Text = "Таблицы созданы.\n";
            ResultDataGrid.ItemsSource = null;
            TotalCostTextBlock.Text = "Общая стоимость: ";
        }

        private void FillExample_Click(object sender, RoutedEventArgs e)
        {
            SuppliersCountTextBox.Text = "3";
            ConsumersCountTextBox.Text = "3";
            CreateInputTables(3, 3);

            int[,] exampleCosts =
            {
                { 2, 3, 1 },
                { 5, 4, 8 },
                { 5, 6, 8 }
            };

            int[] exampleSupply = { 30, 40, 20 };
            int[] exampleDemand = { 20, 30, 40 };

            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    costBoxes[i, j].Text = exampleCosts[i, j].ToString();

            for (int i = 0; i < 3; i++)
                supplyBoxes[i].Text = exampleSupply[i].ToString();

            for (int j = 0; j < 3; j++)
                demandBoxes[j].Text = exampleDemand[j].ToString();

            LogTextBox.Text += "Тестовые данные заполнены.\n";
        }

        private void Solve_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (costBoxes == null)
                {
                    MessageBox.Show("Сначала создайте таблицы.");
                    return;
                }

                int m = supplyBoxes.Length;
                int n = demandBoxes.Length;

                int[,] costs = new int[m, n];
                int[] supply = new int[m];
                int[] demand = new int[n];

                for (int i = 0; i < m; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        if (!int.TryParse(costBoxes[i, j].Text, out costs[i, j]) || costs[i, j] < 0)
                        {
                            MessageBox.Show($"Некорректное значение стоимости в ячейке [{i + 1},{j + 1}].");
                            return;
                        }
                    }
                }

                for (int i = 0; i < m; i++)
                {
                    if (!int.TryParse(supplyBoxes[i].Text, out supply[i]) || supply[i] < 0)
                    {
                        MessageBox.Show($"Некорректный запас у поставщика A{i + 1}.");
                        return;
                    }
                }

                for (int j = 0; j < n; j++)
                {
                    if (!int.TryParse(demandBoxes[j].Text, out demand[j]) || demand[j] < 0)
                    {
                        MessageBox.Show($"Некорректная потребность у потребителя B{j + 1}.");
                        return;
                    }
                }

                int sumSupply = supply.Sum();
                int sumDemand = demand.Sum();

                if (sumSupply != sumDemand)
                {
                    MessageBox.Show($"Задача не закрытая.\nСумма запасов = {sumSupply}\nСумма потребностей = {sumDemand}");
                    return;
                }

                int[,] plan = NorthwestCornerMethod(costs, supply, demand, out int totalCost, out string log);

                ShowResult(plan, totalCost);

                lastPlan = plan;
                lastCosts = costs;
                lastSupply = supply;
                lastDemand = demand;
                lastTotalCost = totalCost;

                LogTextBox.Text = log;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при решении задачи: " + ex.Message);
            }
        }

        private int[,] NorthwestCornerMethod(int[,] costs, int[] supplyInput, int[] demandInput, out int totalCost, out string log)
        {
            int m = supplyInput.Length;
            int n = demandInput.Length;

            int[] supply = (int[])supplyInput.Clone();
            int[] demand = (int[])demandInput.Clone();

            int[,] plan = new int[m, n];
            totalCost = 0;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Ход решения методом северо-западного угла:");
            sb.AppendLine();

            int i = 0;
            int j = 0;

            while (i < m && j < n)
            {
                int x = Math.Min(supply[i], demand[j]);
                plan[i, j] = x;

                int cellCost = x * costs[i, j];
                totalCost += cellCost;

                sb.AppendLine($"Шаг: X[{i + 1},{j + 1}] = min({supply[i]}, {demand[j]}) = {x}");
                sb.AppendLine($"Стоимость в клетке: {x} * {costs[i, j]} = {cellCost}");

                supply[i] -= x;
                demand[j] -= x;

                sb.AppendLine($"Остаток запаса A{i + 1}: {supply[i]}");
                sb.AppendLine($"Остаток потребности B{j + 1}: {demand[j]}");
                sb.AppendLine();

                if (supply[i] == 0 && demand[j] == 0)
                {
                    i++;
                    j++;
                }
                else if (supply[i] == 0)
                {
                    i++;
                }
                else
                {
                    j++;
                }
            }

            sb.AppendLine($"Итоговая общая стоимость: {totalCost}");
            log = sb.ToString();

            return plan;
        }

        private void ShowResult(int[,] plan, int totalCost)
        {
            int m = plan.GetLength(0);
            int n = plan.GetLength(1);

            DataTable table = new DataTable();

            for (int j = 0; j < n; j++)
                table.Columns.Add($"B{j + 1}");

            for (int i = 0; i < m; i++)
            {
                DataRow row = table.NewRow();
                for (int j = 0; j < n; j++)
                    row[j] = plan[i, j];
                table.Rows.Add(row);
            }

            ResultDataGrid.ItemsSource = table.DefaultView;
            TotalCostTextBlock.Text = $"Общая стоимость: {totalCost}";
        }

        private void ExportCsv_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (lastPlan == null)
                {
                    MessageBox.Show("Сначала решите задачу.");
                    return;
                }

                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "CSV file (*.csv)|*.csv",
                    FileName = "transport_result.csv"
                };

                if (saveFileDialog.ShowDialog() != true)
                    return;

                StringBuilder sb = new StringBuilder();

                sb.AppendLine("План перевозок");
                for (int i = 0; i < lastPlan.GetLength(0); i++)
                {
                    for (int j = 0; j < lastPlan.GetLength(1); j++)
                    {
                        sb.Append(lastPlan[i, j]);
                        if (j < lastPlan.GetLength(1) - 1)
                            sb.Append(";");
                    }
                    sb.AppendLine();
                }

                sb.AppendLine();
                sb.AppendLine($"Общая стоимость;{lastTotalCost}");
                sb.AppendLine();
                sb.AppendLine("Лог расчета");
                sb.AppendLine(LogTextBox.Text.Replace(Environment.NewLine, " "));

                File.WriteAllText(saveFileDialog.FileName, sb.ToString(), Encoding.UTF8);
                MessageBox.Show("Экспорт выполнен успешно.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка экспорта: " + ex.Message);
            }
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            SuppliersCountTextBox.Clear();
            ConsumersCountTextBox.Clear();

            CostsGrid.Children.Clear();
            CostsGrid.RowDefinitions.Clear();
            CostsGrid.ColumnDefinitions.Clear();

            SupplyPanel.Children.Clear();
            DemandPanel.Children.Clear();

            ResultDataGrid.ItemsSource = null;
            TotalCostTextBlock.Text = "Общая стоимость: ";
            LogTextBox.Clear();

            costBoxes = null;
            supplyBoxes = null;
            demandBoxes = null;

            lastPlan = null;
            lastCosts = null;
            lastSupply = null;
            lastDemand = null;
            lastTotalCost = 0;
        }
    }
}