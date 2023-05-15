using Microsoft.Data.SqlClient;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using MessageBox = System.Windows.MessageBox;

namespace XSD_Mapper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private XDocument? xmlDoc;

        private MappingData mappingData = new();

        public MainWindow()
        {
            InitializeComponent();
        }

        #region Buttons

        private void SelectXmlFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var openFileDialog = new Microsoft.Win32.OpenFileDialog();
                openFileDialog.Filter = "XML Files (*.xml)|*.xml";
                openFileDialog.Title = "Select XML File";
                if (openFileDialog.ShowDialog() == true)
                {
                    xmlSource.Text = openFileDialog.FileName;
                    LoadXmlDocument(openFileDialog.FileName); // Load the XML document
                    //LoadXmlNodes(); // Populate the TreeView with XML nodes
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error selecting XML file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void SelectDatabase_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ConnectionWindow connectionWindow = new ConnectionWindow();
                if (connectionWindow.ShowDialog() == true)
                {
                    connectionString.Text = Config.ConnectionString;
                    LoadDatabaseTablesToComboBox();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error selecting database: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddMapping_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedTable = tableSel.SelectedItem?.ToString();
                var selectedColumn = columnSel.SelectedItem?.ToString();
                var selectedNode = xmlNodes.SelectedItem as TreeViewItem; // Changed to TreeViewItem

                if (selectedTable == null || selectedColumn == null || selectedNode == null) return;

                mappingData.Mappings ??= new List<Mapping>();
                var existingMapping = mappingData.Mappings.FirstOrDefault(m => m.TableName == selectedTable);

                if (existingMapping != null)
                {
                    // Mapping already exists for this table, add new column to it
                    existingMapping.Columns.Add(new Column
                    {
                        ColumnName = selectedColumn,
                        XmlNodePath = GetXmlNodePath(selectedNode)
                    });
                }
                else
                {
                    // No existing mapping for this table, create a new one
                    var mapping = new Mapping
                    {
                        TableName = selectedTable,
                        Columns = new List<Column>
                {
                    new Column
                    {
                        ColumnName = selectedColumn,
                        XmlNodePath = GetXmlNodePath(selectedNode)
                    }
                }
                    };
                    mappingData.Mappings.Add(mapping);
                }

                // Refresh the ListBox
                mappingsListb.Items.Clear();
                foreach (var mapping in mappingData.Mappings)
                {
                    foreach (var column in mapping.Columns)
                    {
                        mappingsListb.Items.Add($"{mapping.TableName} {column.ColumnName} -> {column.XmlNodePath}");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding mapping: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RemoveMapping_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedMapping = mappingsListb.SelectedItem?.ToString();
                if (selectedMapping == null) return;
                var mappingParts = selectedMapping.Split(" -> ");
                var tableName = mappingParts[0].Split(" ")[0];
                var columnName = mappingParts[0].Split(" ")[1];
                var xmlNodePath = mappingParts[1];
                var mapping = mappingData.Mappings.FirstOrDefault(m => m.TableName == tableName);
                if (mapping == null) return;
                var column = mapping.Columns.FirstOrDefault(c => c.ColumnName == columnName && c.XmlNodePath == xmlNodePath);
                if (column == null) return;
                mapping.Columns.Remove(column);
                // Refresh the ListBox
                mappingsListb.Items.Clear();
                foreach (var m in mappingData.Mappings)
                {
                    foreach (var c in m.Columns)
                    {
                        mappingsListb.Items.Add($"{m.TableName} {c.ColumnName} -> {c.XmlNodePath}");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error removing mapping: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveMappingFileToJson_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "JSON Files (*.json)|*.json";
                saveFileDialog.Title = "Save Mapping File";
                if (saveFileDialog.ShowDialog() == true)
                {
                    var json = Newtonsoft.Json.JsonConvert.SerializeObject(mappingData);
                    System.IO.File.WriteAllText(saveFileDialog.FileName, json);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving mapping file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Loads
        private void LoadXmlNodesToTreeView(XElement element, TreeViewItem parentItem)
        {
            var newItem = new TreeViewItem { Header = element.Name.LocalName };
            parentItem.Items.Add(newItem);
            foreach (var childElement in element.Elements())
            {
                LoadXmlNodesToTreeView(childElement, newItem);
            }
        }

        private void LoadXmlDocument(string xmlFilePath)
        {
            try
            {
                xmlDoc = XDocument.Load(xmlFilePath);
                LoadXsdToEditor();
                var rootNode = new TreeViewItem { Header = xmlDoc.Root.Name.LocalName };
                xmlNodes.Items.Add(rootNode);
                foreach (var childElement in xmlDoc.Root.Elements())
                {
                    LoadXmlNodesToTreeView(childElement, rootNode);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading XML document: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadDatabaseTablesToComboBox()
        {
            try
            {
                using var connection = new SqlConnection(connectionString.Text);
                connection.Open();
                var tables = connection.GetSchema("Tables").AsEnumerable()
                    .Select(row => row.Field<string>("TABLE_NAME"))
                    .OrderBy(name => name) // sort alphabetically
                    .ToList();
                tableSel.ItemsSource = tables;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading database tables: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadDatabaseColumnsToCombobox()
        {
            var selectedTable = tableSel.SelectedItem?.ToString();
            if (selectedTable == null) return;

            try
            {
                using var connection = new SqlConnection(connectionString.Text);
                connection.Open();
                var columns = connection.GetSchema("Columns", new string[] { null, null, selectedTable })
                    .AsEnumerable()
                    .Select(row => row.Field<string>("COLUMN_NAME"))
                    .ToList();
                columnSel.ItemsSource = columns;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading table columns: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadXsdToEditor()
        {
            try
            {
                using XmlReader reader = xmlDoc.CreateReader();
                XmlSchemaSet schemaSet = new XmlSchemaSet();
                XmlSchemaInference schemaInference = new XmlSchemaInference();
                schemaInference.InferSchema(reader, schemaSet);

                // Retrieve the inferred schema from the XmlSchemaSet object
                XmlSchema? schema = schemaSet.Schemas().Cast<XmlSchema>().FirstOrDefault();

                // Write the schema to textEditor
                using var writer = new System.IO.StringWriter();
                if (schema != null)
                {
                    schema.Write(writer);
                    textEditor.Text = writer.ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating XSD: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region Methods

        private static string GetXmlNodePath(TreeViewItem item)
        {
            string? path = item.Header.ToString();
            var parent = item.Parent as TreeViewItem;
            while (parent != null)
            {
                path = parent.Header + "/" + path;
                parent = parent.Parent as TreeViewItem;
            }
            return path;
        }


        #endregion

        #region Events

        private void tableSel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadDatabaseColumnsToCombobox();
        }

        private void textEditor_DocumentChanged(object sender, EventArgs e)
        {

        }

        #endregion

    }
}