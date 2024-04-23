using Ac4.DTOs;
using Ac4.Persistencia.Mappings;
using Ac4.Persistencia.Utils;
using CsvHelper;
using Npgsql;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;

namespace Ac4
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        ConsumMap consumMap = new ConsumMap();

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                SetDataSource();
            }
            catch (Exception exc) {
                MessageBox.Show($"Error con la conexi�n al cloud. EXCEPTION: {exc.Message}", "Error");
            }
            
            SetYears();
            if (File.Exists("output.xml"))
                File.Delete("output.xml");
            SaveToXml();
            PopulateComboBoxFromXml();
        }

        private void dataGridCSVOutput_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow selectedRow = dataGridCSVOutput.Rows[e.RowIndex];
                ConsumDTO selectedConsum = new ConsumDTO
                {
                    Any = Convert.ToInt32(selectedRow.Cells["Any"].Value),
                    CodComarca = Convert.ToInt32(selectedRow.Cells["Codi Comarca"].Value),
                    Comarca = selectedRow.Cells["Comarca"].Value.ToString(),
                    Poblacio = Convert.ToInt32(selectedRow.Cells["Poblaci�"].Value),
                    XarxaDomestica = Convert.ToInt32(selectedRow.Cells["Dom�stic xarxa"].Value),
                    ActivitatsEconomiques = Convert.ToInt32(selectedRow.Cells["Activitats econ�miques i fonts pr�pies"].Value),
                    Total = Convert.ToInt32(selectedRow.Cells["Total"].Value),
                    ConsumPerCapita = Convert.ToSingle(selectedRow.Cells["Consum dom�stic per c�pita"].Value)
                };
                UpdateStats(selectedConsum);
            }
        }

        private void buttonClear_Click(object sender, EventArgs e)
        {
            comboBoxAny.SelectedItem = null;
            comboBoxComarca.SelectedItem = null;
            textBoxClear.Text = string.Empty;
            textBoxClear2.Text = string.Empty;
            textBoxClear3.Text = string.Empty;
            textBoxClear4.Text = string.Empty;
            textBoxClear5.Text = string.Empty;
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            Regex intpattern = new Regex(@"^[0-9]{1,9}$");
            Regex floatpattern = new Regex(@"^[.][0-9]+$|^[0-9]*[.]{0,2}[0-9]*$");
            if (!intpattern.IsMatch(textBoxClear.Text) || !intpattern.IsMatch(textBoxClear2.Text) ||
                !intpattern.IsMatch(textBoxClear3.Text) || !intpattern.IsMatch(textBoxClear4.Text) ||
                !floatpattern.IsMatch(textBoxClear5.Text) || textBoxClear5.Text == string.Empty)
            {
                MessageBox.Show("Uno de los valores introducidos no concuerda con su tipo o los campos est�n vac�os.", "Error");
            }
            else if (comboBoxAny.SelectedItem == null || comboBoxComarca.SelectedItem == null)
            {
                MessageBox.Show("Debes seleccionar los datos de la lista.", "Error");
            }
            else
            {
                ConsumDTO fieldsObject = new ConsumDTO();
                fieldsObject.Any = int.Parse(comboBoxAny.Text);
                fieldsObject.CodComarca = getComarcaCode(comboBoxComarca.Text);
                fieldsObject.Comarca = comboBoxComarca.Text;
                fieldsObject.Poblacio = int.Parse(textBoxClear.Text);
                fieldsObject.XarxaDomestica = int.Parse(textBoxClear2.Text);
                fieldsObject.ActivitatsEconomiques = int.Parse(textBoxClear3.Text);
                fieldsObject.Total = int.Parse(textBoxClear4.Text);
                fieldsObject.ConsumPerCapita = float.Parse(textBoxClear5.Text);

                consumMap.InsertRecord(fieldsObject);
                SetDataSource();
            }
        }

        // No se utiliza, es de la �ltima AC pero sirve para cargar los datos del CSV en una lista, util para cargar del csv al cloud.
        List<ConsumDTO> ReadCsv()
        {
            List<ConsumDTO> resultList = new List<ConsumDTO>();
            using StreamReader reader = new StreamReader("input.csv");
            using CsvReader csvreader = new CsvReader(reader, CultureInfo.InvariantCulture);
            csvreader.Read();
            csvreader.ReadHeader();
            while (csvreader.Read())
            {
                var record = new ConsumDTO
                {
                    Any = csvreader.GetField<int>("Any"),
                    CodComarca = csvreader.GetField<int>("Codi comarca"),
                    Comarca = csvreader.GetField<string>("Comarca"),
                    Poblacio = csvreader.GetField<int>("Poblaci�"),
                    XarxaDomestica = csvreader.GetField<int>("Dom�stic xarxa"),
                    ActivitatsEconomiques = csvreader.GetField<int>("Activitats econ�miques i fonts pr�pies"),
                    Total = csvreader.GetField<int>("Total"),
                    ConsumPerCapita = csvreader.GetField<float>("Consum dom�stic per c�pita")
                };
                resultList.Add(record);
            }
            return resultList;
        }

        void SetYears()
        {
            // Changing year value range.
            List<ConsumDTO> recordsList = consumMap.GetAllRecords();
            ConsumDTO lowestYear = recordsList.OrderBy(c => c.Any).First();
            int maxRange = 2050;
            for (int i = lowestYear.Any; i < maxRange + 1; i++)
            {
                comboBoxAny.Items.Add(i.ToString());
            }
        }

        void PopulateComboBoxFromXml()
        {
            comboBoxComarca.Items.Clear();
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load("output.xml");
                XmlNodeList comarcaNodes = xmlDoc.SelectNodes("//ComarcaItem");
                foreach (XmlNode comarcaNode in comarcaNodes)
                {
                    string comarca = comarcaNode.SelectSingleNode("Comarca").InnerText;
                    comboBoxComarca.Items.Add(comarca);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error reading XML file: " + ex.Message, "Error");
            }
        }

        void SaveToXml()
        {
            List<ConsumDTO> recordsList = consumMap.GetAllRecords();
            XmlWriterSettings settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  ",
                NewLineChars = "\r\n",
                NewLineHandling = NewLineHandling.Replace
            };

            HashSet<int> uniqueCodComarcas = new HashSet<int>();

            using (XmlWriter output = XmlWriter.Create("output.xml", settings))
            {
                output.WriteStartDocument();

                output.WriteStartElement("Comarcas");
                foreach (ConsumDTO record in recordsList)
                {
                    if (!uniqueCodComarcas.Contains(record.CodComarca))
                    {
                        output.WriteStartElement("ComarcaItem");
                        output.WriteElementString("CodComarca", record.CodComarca.ToString());
                        output.WriteElementString("Comarca", record.Comarca.ToString());
                        output.WriteEndElement();
                        uniqueCodComarcas.Add(record.CodComarca);
                    }
                }
                output.WriteEndElement();
            }
        }


        private int getComarcaCode(string comarca)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load("output.xml");
            XmlNodeList comarcaNodes = xmlDoc.SelectNodes("//ComarcaItem");
            foreach (XmlNode comarcaNode in comarcaNodes)
            {
                string comarcaf = comarcaNode.SelectSingleNode("Comarca").InnerText;
                if (comarcaf == comarca)
                    return int.Parse(comarcaNode.SelectSingleNode("CodComarca").InnerText);
            }
            return -1;
        }

        private void UpdateStats(ConsumDTO selected)
        {
            List<ConsumDTO> csvdata = consumMap.GetAllRecords();

            textBox20kAbove.Text = selected.Poblacio > 20000 ? "S�" : "No";

            textBoxAvg.Text = (selected.XarxaDomestica / selected.Poblacio).ToString();

            textBoxLowerDom.Text = FindRecordWithLowestConsumPerCapita(csvdata).ConsumPerCapita == selected.ConsumPerCapita ? "S�" : "No";

            textBoxHigherDom.Text = FindRecordWithHighestConsumPerCapita(csvdata).ConsumPerCapita == selected.ConsumPerCapita ? "Si" : "No";
        }

        private ConsumDTO FindRecordWithHighestConsumPerCapita(List<ConsumDTO> consums)
        {
            ConsumDTO highestConsumPerCapitaRecord = consums.OrderByDescending(x => x.ConsumPerCapita).First();
            return highestConsumPerCapitaRecord;
        }

        private ConsumDTO FindRecordWithLowestConsumPerCapita(List<ConsumDTO> consums)
        {
            ConsumDTO lowestConsumPerCapitaRecord = consums.OrderBy(x => x.ConsumPerCapita).First();

            return lowestConsumPerCapitaRecord;
        }

        private void SetDataSource()
        {
            List<ConsumDTO> resultList = consumMap.GetAllRecords();
            DataTable dt = new();

            dt.Columns.Add("Any", typeof(int));
            dt.Columns.Add("Codi comarca", typeof(int));
            dt.Columns.Add("Comarca", typeof(string));
            dt.Columns.Add("Poblaci�", typeof(int));
            dt.Columns.Add("Dom�stic xarxa", typeof(int));
            dt.Columns.Add("Activitats econ�miques i fonts pr�pies", typeof(int));
            dt.Columns.Add("Total", typeof(int));
            dt.Columns.Add("Consum dom�stic per c�pita", typeof(float));

            foreach (ConsumDTO value in resultList)
            {
                dt.Rows.Add(value.Any, value.CodComarca, value.Comarca, value.Poblacio, value.XarxaDomestica, value.ActivitatsEconomiques, value.Total, value.ConsumPerCapita);
            }

            dataGridCSVOutput.DataSource = dt;
        }
    }
}
