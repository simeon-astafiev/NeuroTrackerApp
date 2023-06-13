using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TogiSoft.DataControls;
using TogiSoft.IO;

namespace UserAlgoritmStarter
{
    [ToolboxItem(false)]

    /// <summary>
    /// Содержит функционал для работы с Neurotracker - выбор метода машинного обучения, установка гиперпараметров, обучение и получение результатов работы метода машинного обучения
    /// </summary>
    public partial class UserAlgoritmControl : TogiUserControl
    {
        /// <summary>
        /// Название папки с файлами программы в %temp%
        /// </summary>
        private const string neurotrackerFiles = "Neurotracker files";

        /// <summary>
        /// Расположение quantumtable, необходимой для работы сторонней программы
        /// </summary>
        private const string quantumtable = "C:\\Users\\Астафьев Семён\\Desktop\\Практика\\quantumtable.csv";

        /// <summary>
        /// Значение "value"
        /// </summary>
        private const string value = "value";

        /// <summary>
        /// Значение "name"
        /// </summary>
        private const string name = "name";

        /// <summary>
        /// Процесс сторонней программы
        /// </summary>
        private Process process;

        /// <summary>
        /// Объект для блокировки потоков
        /// </summary>
        private object locker = new object();

        /// <summary>
        /// Токен для отмены
        /// </summary>
        private CancellationTokenSource source;

        /// <summary>
        /// Инициалированный UserAlgoritmControl
        /// </summary>
        public UserAlgoritmControl()
        {
            InitializeComponent();
            Log.Info("Test");
        }

        /// <summary>
        /// Создание интерфейса по JSON
        /// </summary>
        /// <param name="jsonPath"> Путь к JSON файлу </param>
        /// <remarks> Перебираем все "hyperparameters" в json;
        /// проверяем, содержит ли свойство "value" элемента hyperparameters своё собственное "value";
        /// если да -это должен быть groupbox с несколькими гиперпараметрами;
        /// если нет -это должен быть groupbox с одним гиперпараметром;
        /// конечные "value" гиперпараметров могут быть трёх типов - jsonvaluekind.number, jsonvaluekind.string, jsonvaluekind.array;
        /// для number предназначен inputcontrol, для string и array - listcontrol </remarks>
        public void CreateFromJson(string jsonPath)
        {
            var jsonString = File.ReadAllText(jsonPath);
            var json = JsonSerializer.Deserialize<JsonElement>(jsonString);
            foreach (var hyperparameter in json.GetProperty("hyperparameters").EnumerateArray().Reverse())
            {
                var hyperparameterValue = hyperparameter.GetProperty(value);
                var containsValues = hyperparameterValue.ToString().Contains(value);
                if (containsValues)
                {
                    var groupbox = new GroupBox();
                    groupbox.AutoSize = true;
                    groupbox.Anchor = AnchorStyles.Bottom;
                    groupbox.Text = hyperparameter.GetProperty(name).ToString();

                    var panel = new FlowLayoutPanel();
                    panel.Anchor = AnchorStyles.Bottom;
                    panel.AutoScroll = true;

                    foreach (var nestedHyperparameter in hyperparameterValue.EnumerateArray())
                    {
                        var nestedHyperparameterType = nestedHyperparameter.GetProperty(value).ValueKind;
                        if (nestedHyperparameterType == JsonValueKind.Number)
                        {
                            var inputControl = new InputControl(nestedHyperparameter.GetProperty(name).ToString(), nestedHyperparameter.GetProperty(value).GetDecimal());
                            panel.Controls.Add(inputControl);
                            inputControl.Visible = true;
                            inputControl.Dock = DockStyle.Top;
                    }
                        else if (nestedHyperparameterType == JsonValueKind.String)
                        {
                            var listControl = new ListControl(nestedHyperparameter.GetProperty(name).ToString(), nestedHyperparameter.GetProperty(value));
                            panel.Controls.Add(listControl);
                            listControl.Visible = true;
                            listControl.Dock = DockStyle.Top;
                        }
                        else if (hyperparameter.GetProperty(value).ValueKind == JsonValueKind.Array)
                        {
                            var listControl = new ListControl(nestedHyperparameter.GetProperty(name).ToString(), nestedHyperparameter.GetProperty(value));
                            panel.Controls.Add(listControl);
                            listControl.Visible = true;
                            listControl.Dock = DockStyle.Top;
                        }

                        groupbox.Controls.Add(panel);
                        panel.Dock = DockStyle.Top;
                        xtraScrollableControl1.Controls.Add(groupbox);
                        groupbox.Dock = DockStyle.Top;
                    }
                }
                else
                {
                    var groupbox = new GroupBox();
                    groupbox.Anchor = AnchorStyles.Bottom;
                    groupbox.Text = hyperparameter.GetProperty(name).ToString();

                    var panel = new FlowLayoutPanel();
                    panel.Anchor = AnchorStyles.Bottom;
                    panel.AutoScroll = true;

                    if (hyperparameter.GetProperty(value).ValueKind == JsonValueKind.Number)
                    {
                        var inputControl = new InputControl(hyperparameter.GetProperty(name).ToString(), hyperparameter.GetProperty(value).GetDecimal());
                        panel.Controls.Add(inputControl);
                        inputControl.Visible = true;
                        inputControl.Dock = DockStyle.Top;
                    }
                    else if (hyperparameter.GetProperty(value).ValueKind == JsonValueKind.String)
                    {
                        var listControl = new ListControl(hyperparameter.GetProperty(name).ToString(), hyperparameter.GetProperty(value));
                        panel.Controls.Add(listControl);
                        listControl.Visible = true;
                        listControl.Dock = DockStyle.Top;
                    }
                    else if (hyperparameter.GetProperty(value).ValueKind == JsonValueKind.Array)
                    {
                        var listControl = new ListControl(hyperparameter.GetProperty(name).ToString(), hyperparameter.GetProperty(value));
                        panel.Controls.Add(listControl);
                        listControl.Visible = true;
                        listControl.Dock = DockStyle.Top;
                    }

                    groupbox.Controls.Add(panel);
                    panel.Dock = DockStyle.Top;
                    xtraScrollableControl1.Controls.Add(groupbox);
                    groupbox.Dock = DockStyle.Top;
                }
            }
        }

        /// <summary>
        /// Создание JSON из заполненных полей динамически созданного интерфейса
        /// </summary>
        /// <remarks>Перебираем все Groupbox'ы;
        /// Если панель внутри groupbox содержит один control - записываем в JSON одной строкой имя и значение гиперпараметра;
        /// Иначе создаем отдельный JSON, в который построчно записываем имена и значения содержащихся в нём гиперпараметров, а затем вкладываем его в основной JSON</remarks>remarks>
        public void MakeJson()
        {
            var jsonObj = new JsonObject();
            foreach (var groupBox in xtraScrollableControl1.Controls.OfType<GroupBox>().Reverse())
            {
                var controlsCount = groupBox.Controls[0].Controls.Count;
                var groupName = groupBox.Text;
                if (controlsCount == 1)
                {
                    foreach (var inputControl in groupBox.Controls[0].Controls.OfType<InputControl>())
                    {
                        var controlValue = inputControl.GetSpinValue();
                        jsonObj.Add(groupName, controlValue);
                    }

                    foreach (var listControl in groupBox.Controls[0].Controls.OfType<ListControl>())
                    {
                        var controlValue = listControl.GetComboText();
                        jsonObj.Add(groupName, controlValue);
                    }
                }
                else
                {
                    var valueObj = new JsonObject();
                    foreach (var inputControl in groupBox.Controls[0].Controls.OfType<InputControl>())
                    {
                        var hyperparameterName = inputControl.GetLabelText();
                        var controlValue = inputControl.GetSpinValue();
                        valueObj.Add(hyperparameterName, controlValue);
                    }

                    foreach (var listControl in groupBox.Controls[0].Controls.OfType<ListControl>())
                    {
                        var hyperparameterName = listControl.GetLabelText();
                        var controlValue = listControl.GetComboText();
                    }

                    jsonObj.Add(groupName, valueObj);
                }
            }

            string outputJson = JsonSerializer.Serialize(jsonObj, new JsonSerializerOptions { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
            File.WriteAllText(Path.Combine(Path.GetTempPath(), neurotrackerFiles + "\\hyperparameters.json"), outputJson);
        }

        /// <summary>
        /// Нажатие на кнопку OpenFile.
        /// </summary>
        /// <param name="sender"> Sender </param>
        /// <param name="e"> Event </param>
        private void simpleButton1_Click(object sender, System.EventArgs e)
        {
            openFileDialog1.ShowDialog();
        }

        /// <summary>
        /// Подтверждение выбора файла в диалоговом окне.
        /// </summary>
        /// <param name="sender"> Sender </param>
        /// <param name="e"> Event </param>
        /// <remarks>Производится предварительная очистка временной папки, если она уже существует.
        /// Идет запуск сторонней программы с аргументами для получения config.json
        /// Происходит создание интерфейса по полученному файлу config.json</remarks>
        private async void xtraOpenFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            xtraScrollableControl1.Controls.Clear();
            var exist = Directory.Exists(Path.Combine(Path.GetTempPath(), neurotrackerFiles));
            if (exist)
            {
                System.IO.DirectoryInfo di = new DirectoryInfo(Path.Combine(Path.GetTempPath(), neurotrackerFiles));

                foreach (FileInfo tempFile in di.GetFiles())
                {
                    tempFile.Delete();
                }
            }

            string pickedFile = openFileDialog1.FileName;
            textBox1.Text = pickedFile;
            string tempPath = Path.Combine(Path.GetTempPath(), neurotrackerFiles);
            Directory.CreateDirectory(tempPath);
            tempPath = $@"""{tempPath}""";
            var getconfig = "/getconfig" + " " + tempPath + "\\config.json";
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = pickedFile;
            psi.WorkingDirectory = Path.GetDirectoryName(pickedFile);
            psi.Arguments = getconfig;
            await Task.Run(() =>
            {
                var process = Process.Start(psi);
                process.WaitForExit();
                process.Dispose();
            });
            CreateFromJson(Path.Combine(Path.GetTempPath(), neurotrackerFiles + "\\config.json"));
            simpleButton2.Enabled = true;

            await Task.Run(() => File.Copy(quantumtable, Path.Combine(Path.GetTempPath(), neurotrackerFiles + "\\quantumtable.csv"), true));
        }

        /// <summary>
        /// Нажатие на кнопку Старт
        /// </summary>
        /// <param name="sender"> Sender </param>
        /// <param name="e"> Event </param>
        /// <remarks>Запуск сторонней программы с аргументами для обучения.
        /// Запуск сторонней  программы с аргументами для предсказания. </remarks>
        private async void simpleButton2_Click(object sender, EventArgs e)
        {
            if (source != null)
            {
                source.Dispose();
            }

            source = new CancellationTokenSource();
            try
            {
                simpleButton2.Enabled = false;
                MakeJson();
                var tempPath = Path.Combine(Path.GetTempPath(), neurotrackerFiles);
                Directory.CreateDirectory(tempPath);
                tempPath = $@"""{tempPath}""";
                var learn = "/learn" + " " + tempPath + "\\hyperparameters.json" + " " + tempPath + "\\quantumtable.csv" + " " + tempPath + "\\savemodel.dat";
                var psi = new ProcessStartInfo();
                psi.FileName = textBox1.Text;
                psi.WorkingDirectory = Path.GetDirectoryName(textBox1.Text);
                psi.Arguments = learn;

                await Task.Run(() =>
                {
                    lock (locker)
                    {
                        if (source.IsCancellationRequested)
                        {
                            MessageBox.Show("Отмена 1");
                            return;
                        }

                        process = Process.Start(psi);
                    }

                    if (process is null)
                    {
                        return;
                    }

                    process.WaitForExit();

                    lock (locker)
                    {
                        process.Dispose();
                    }
                });

                if (source.IsCancellationRequested)
                {
                    MessageBox.Show("Отмена 2");
                    return;
                }

                var predict = "/predict" + " " + tempPath + "\\savemodel.dat" + " " + tempPath + "\\quantumtable.csv" + " " + tempPath + "\\predicts.csv";
                var psi2 = new ProcessStartInfo();
                psi2.FileName = textBox1.Text;
                psi2.WorkingDirectory = Path.GetDirectoryName(textBox1.Text);
                psi2.Arguments = predict;

                await Task.Run(() =>
                {
                    lock (locker)
                    {
                        if (source.IsCancellationRequested)
                        {
                            MessageBox.Show("Отмена 3");
                            return;
                        }

                        process = Process.Start(psi2);
                    }

                    if (process is null)
                    {
                        return;
                    }

                    process.WaitForExit();

                    lock (locker)
                    {
                        process.Dispose();
                    }
                });
                simpleButton2.Enabled = true;
                var predictsFolder = neurotrackerFiles + "\\predicts.csv";
                var predictsPath = Path.Combine(Path.GetTempPath(), predictsFolder);
                var exist = File.Exists(predictsPath);
                if (exist)
                {
                    MessageBox.Show("Получены предсказания");
                }
                else
                {
                    MessageBox.Show("Предсказания не найдены");
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show($"{exception}");
                Log.Error("Ошибка", ex: exception);
            }
        }

        /// <summary>
        /// Нажатие на кнопку Отмена
        /// </summary>
        /// <param name="sender"> Sender </param>
        /// <param name="e"> Event </param>
        private void simpleButton3_Click(object sender, EventArgs e)
        {
            lock (locker)
            {
                source.Cancel();
                if (process != null)
                {
                    process.CloseMainWindow();
                    simpleButton2.Enabled = true;
                }
            }
        }
    }
}