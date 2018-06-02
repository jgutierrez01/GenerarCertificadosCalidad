using Microsoft.Reporting.WinForms;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GeneradorCertificados
{

    public partial class Form1 : Form
    {
        private FileStream DocumentoActual = null;
        string name = null;
        string file = null;
        string namesalida = null;
        string filesalida = null;
        ArrayList listado = new ArrayList();//guarda el flujo de datos
        string directorio;
        SqlConnection conexion = new SqlConnection(Properties.Settings.Default.conexionproyectoid);
        System.Drawing.Color Barcolor = System.Drawing.Color.Blue;
        string url = string.Empty;
        public Form1()
        {
            InitializeComponent();
        }
        private void button1_Click(object sender, EventArgs e)
        {

        }
        private void btnExaminar_Click_1(object sender, EventArgs e)
        {

            abrirarchivo.FileName = "";
            if (ListaProyecto.Text != "")
            {
                if (abrirarchivo.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    name = abrirarchivo.SafeFileNames[0];
                    file = abrirarchivo.FileNames[0];
                    txtRutaEntrada.Text = "\"" + file + "\"";
                    btnRutaSalida.Enabled = true;
                }
            }
            else
            {
                MessageBox.Show(this, "Seleccione Un Proyecto");
            }
        }
        private void btnRutaSalida_Click_1(object sender, EventArgs e)
        {
            //guardararchivo.Title = "Guardar Archivo en ...";
            //guardararchivo.RestoreDirectory = true;
            if (folderBrowserDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string salida = folderBrowserDialog1.SelectedPath;
                filesalida = folderBrowserDialog1.SelectedPath;
                ////////////////////////77
                //FileInfo fi = new FileInfo(guardararchivo.FileName);//obtiene informacion hacer de la ruta del archivo
                //filesalida = fi.DirectoryName;
                //namesalida = fi.Name;

                //string salida = filesalida+ "\\" + namesalida;
                // MessageBox.Show(this,""+salida);


                //directorio = guardararchivo.FileName;
                txtRutaSalida.Text += "\"" + salida + "\"";
            }
            if (txtRutaSalida.Text != "")
            {
                btnReporte.Enabled = true;
            }

        }
        private void clear()
        {
            this.file = null;
            this.name = null;
            txtRutaEntrada.Clear();
            txtRutaSalida.Clear();
            btnExaminar.Enabled = true;
            btnRutaSalida.Enabled = false;
            btnReporte.Enabled = false;
            mostrar1.Text = "";
            listado.Clear();
            conexion.Close();

        }
        private bool leer_archivo(string file)
        {
            bool resultado;
            string cadena;
            if (file == null || file.Equals(""))
            {
                MessageBox.Show(this, "Seleccione un archivo.");
                resultado = false;
            }
            else
            {
                resultado = true;
                Stream ArchivoAbierto = null;

                try
                {
                    ArchivoAbierto = File.OpenRead(this.file);//abrir archivo

                    var reader = new StreamReader(ArchivoAbierto);//canal de lectura
                    int lineno = 1;
                    //Un máximo d
                    while (lineno <= 600000 && !reader.EndOfStream)
                    {

                        var line = reader.ReadLine();
                        var values = line.Split(',');

                        if (lineno >= 1)
                        {
                            cadena = values[0];
                            listado.Add(cadena);
                        }
                        lineno++;
                    }
                    reader.Close();
                    ArchivoAbierto.Close();

                }
                catch (Exception ex)
                { throw ex; }
            }
            return resultado;
        }
        private void btnReporte_Click(object sender, EventArgs e)
        {                           
            DataTable dt = ConvertToDataTable(file, 1);
            if (dt.Rows.Count < 70)
            {
                string var2;
                //try
                //{
                if (leer_archivo(this.file))
                {
                    btnExaminar.Enabled = false;
                    BarraProcesos.Value = 10;
                    btnRutaSalida.Enabled = false;

                    ReportteSalida.ProcessingMode = Microsoft.Reporting.WinForms.ProcessingMode.Remote;//
                    ReportteSalida.ServerReport.ReportServerUrl = new Uri("http://www.samaltamira.net/ReportServer/");
                    this.url = ((Proyecto)ListaProyecto.SelectedItem).RutaEspaniol;
                    string mensaje = "";
                    if (url.Length > 0)
                    {
                        CrearObtenerCSV(filesalida, name.Split('.')[0], out mensaje);

                        ReportteSalida.ServerReport.ReportPath = @"/" + url;//direccion de la consulta de responder
                        string usuario = ConfigurationManager.AppSettings["Usuario"].ToString();
                        string pass = ConfigurationManager.AppSettings["Pass"].ToString();

                        NetworkCredential myCred = new NetworkCredential(usuario, pass, "");
                        ReportteSalida.ServerReport.ReportServerCredentials.NetworkCredentials = myCred;

                        BarraProcesos.Value = 20;
                        mostrar1.Text = "PROCESANDO";
                        int len = listado.Count;
                        int maxleng = 200;
                        int ultimocilo = len % maxleng;
                        int ciclos = ultimocilo == 0 ? (len / maxleng) : (len / maxleng) + 1;
                        ultimocilo = ultimocilo == 0 ? maxleng : ultimocilo;
                        string sincoma = "";
                        // MessageBox.Show(this, "-+++-" + ultimocilo);
                        //MessageBox.Show(this, "-+++-" + ciclos);

                        string formatoSalida = cmbFormatoSalida.SelectedValue.ToString() ;

                        for (int i = 0; i < ciclos; i++)
                        {
                            var2 = "";
                            //int var1 = 0;
                            int limite = ciclos == 1 ? ultimocilo : i == ciclos - 1 ? ultimocilo : maxleng;
                            // MessageBox.Show(this, "*****+" + limite);    
                            for (int j = 0; j < limite; j++)
                            {
                                // MessageBox.Show(this, "--"+(j + i * maxleng));
                                var2 += listado[j + i * maxleng] + ",";

                            }
                            sincoma = var2.TrimEnd(',');
                            string salida = namesalida + "_" + (i + 1) + ".pdf";

                            this.OpSalidaArchivo(sincoma, salida, i, formatoSalida);
                        }
                        btnReporte.Enabled = false;
                        BarraProcesos.Value = 100;
                        CerrarDocumento();
                        MessageBox.Show("Se termino la generación de certificados.");
                        clear();
                        BarraProcesos.Value = 0;


                    }
                    else

                    {
                        MessageBox.Show(this, "Error no existe Reporte Solicitado ");
                        BarraProcesos.Enabled = false;
                        btnReporte.Enabled = false;
                        BarraProcesos.Value = 0;
                        clear();
                        BarraProcesos.Value = 0;
                    }


                }
                else
                {
                    MessageBox.Show(this, "Verifique Formato del archivo ");
                    BarraProcesos.Enabled = false;
                    btnReporte.Enabled = false;
                    BarraProcesos.Value = 0;
                    clear();
                    BarraProcesos.Value = 0;

                }
            }
            else
            {
                MessageBox.Show(this, "El número máximo permitido es 70");
                BarraProcesos.Enabled = false;
                btnReporte.Enabled = false;
                BarraProcesos.Value = 0;
                clear();
                BarraProcesos.Value = 0;
            }                   
        }
        //catch (Exception xe)
        //{
        //    MessageBox.Show(this, "Error de lectura Verifique su archivo, que tenga el formato correcto" );
        //    BarraProcesos.Enabled = false;
        //    btnReporte.Enabled = false;
        //    BarraProcesos.Value = 0;
        //    clear();
        //    BarraProcesos.Value = 0;
        //}
        //}
        private void Form1_Load(object sender, EventArgs e)
        {

            LlenarComboProyecto();
            FormatoClass formato = new FormatoClass();
            FormatoClass formato1 = new FormatoClass();
            List<FormatoClass> lista = new List<FormatoClass>(2);
            
            formato.FormatoID = 1;
            formato.Formato = "PDF";
            formato1.FormatoID = 2;
            formato1.Formato = "EXCEL";
            lista.Add(formato);
            lista.Add(formato1);
            
            cmbFormatoSalida.DataSource = lista;
            cmbFormatoSalida.ValueMember = "FormatoID";
            cmbFormatoSalida.DisplayMember = "Formato";            
        }

        private void BarraProcesos_Click(object sender, EventArgs e)
        {

        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
        }

        public DataTable ConvertToDataTable(string filePath, int numberOfColumns)
        {
            DataTable tbl = new DataTable();

            for (int col = 0; col < numberOfColumns; col++)
                tbl.Columns.Add(new DataColumn("Column" + (col + 1).ToString()));


            string[] lines = System.IO.File.ReadAllLines(filePath);

            foreach (string line in lines)
            {

                var cols = line.Split(',');

                DataRow dr = tbl.NewRow();
                for (int cIndex = 0; cIndex < numberOfColumns; cIndex++)
                {
                    dr[cIndex] = cols[cIndex];
                }

                tbl.Rows.Add(dr);

            }

            return tbl;
        }

        private void OpSalidaArchivo(String sincoma, String nombreArchivo, int i, string formatoSalida)
        {//metodo que interactua con el reporte
            Warning[] warnings = null;
            string filetype = string.Empty;
            string[] streamIds = null;
            string exportType = string.Empty;
            if (formatoSalida == "1")
            {
                exportType = "PDF";
            }
            else if(formatoSalida == "2")
            {
                exportType = "Excel";
            }
            
            string mimeType = string.Empty;
            string encoding = string.Empty;
            string extension = string.Empty;
            //MessageBox.Show(this, "prueba dato" + ListaProyecto.SelectedValue.ToString());
            String[] ListaNumerosControl = sincoma.Split(',');
            foreach (var NumControlActual in ListaNumerosControl)
            {
                try
                {
                    string numeroReporte = ObtenerNumeroReporte(NumControlActual);
                    List<ReportParameter> parametros = new List<ReportParameter>();
                    parametros.Add(new ReportParameter("ProyectoID", ListaProyecto.SelectedValue.ToString()));
                    parametros.Add(new ReportParameter("NumeroReporte", numeroReporte));


                    this.ReportteSalida.ServerReport.SetParameters(parametros);
                    this.ReportteSalida.RefreshReport();

                    filetype = exportType == "" ? "" : exportType;
                    byte[] bytes = ReportteSalida.ServerReport.Render(filetype, null, // deviceinfo not needed for csv
                    out mimeType, out encoding, out extension, out streamIds, out warnings);
                    FileStream fs = null;

                    if (formatoSalida == "1")
                    {
                        fs = new FileStream(filesalida + "\\" + NumControlActual + ".pdf", FileMode.OpenOrCreate);
                    }
                    else if(formatoSalida == "2")
                    {
                        fs = new FileStream(filesalida + "\\" + NumControlActual + ".xls", FileMode.OpenOrCreate);
                    }
                                       
                    fs.Write(bytes, 0, bytes.Length);
                    fs.Close();
                    BarraProcesos.Value = (i + 10);
                }
                catch (ReportServerException ex)
                {
                    this.EscribirMensajeDocumento("", NumControlActual, " No se encontro el numero de control ");
                }
                catch (Exception ex)
                {
                    this.EscribirMensajeDocumento("", NumControlActual, ex.Message);
                }

            }

        }

        public string ObtenerNumeroReporte(string numeroControl)
        {
            string retorno;
            SqlCommand com = new SqlCommand("ObtenerNumeroReporte", conexion);
            com.CommandType = CommandType.StoredProcedure;
            SqlParameter p = com.Parameters.Add("@ProyectoID", SqlDbType.Int);
            p.Value = Convert.ToInt32(ListaProyecto.SelectedValue.ToString());
            SqlParameter pp = com.Parameters.Add("@NumeroControl", SqlDbType.VarChar);
            pp.Value = numeroControl;


            SqlParameter result = new SqlParameter("@retorno", SqlDbType.VarChar, 100);
            result.Direction = ParameterDirection.Output;
            com.Parameters.Add(result);



            conexion.Open();
            com.ExecuteNonQuery();
            retorno = Convert.ToString(result.SqlValue.ToString());
            conexion.Close();

            return retorno;

        }

        public void LlenarComboProyecto()
        {

            conexion.Open();
            DataSet ds = new DataSet();
            SqlDataAdapter da = new SqlDataAdapter("select A.ProyectoID,B.Nombre,A.RutaEspaniol from ProyectoReporte A INNER JOIN Proyecto B ON A.ProyectoID=B.ProyectoID WHERE B.ActivoReporte=1 AND A.TipoReporteProyectoID=4 order by  A.ProyectoID", conexion);
            da.Fill(ds, "Proyecto");
            ListaProyecto.DataSource = ObtenerListaProyectos(ds.Tables[0]);
            ListaProyecto.ValueMember = "ProyectoID";
            ListaProyecto.DisplayMember = "Nombre";
            conexion.Close();
            btnExaminar.Enabled = true;
        }



        private List<Proyecto> ObtenerListaProyectos(DataTable dtProyectos)
        {
            List<Proyecto> listaProyectos = new List<Proyecto>();

            for (int i = 0; i < dtProyectos.Rows.Count; i++)
            {
                listaProyectos.Add(new Proyecto
                {
                    ProyectoID = int.Parse(dtProyectos.Rows[i][0].ToString()),
                    Nombre = dtProyectos.Rows[i][1].ToString(),
                    RutaEspaniol = dtProyectos.Rows[i][2].ToString()
                });
            }
            return listaProyectos;
        }

        public string Reporte()
        {

            SqlCommand com = new SqlCommand("Rpt_ReporteProyectoCertificadoBk", conexion);
            com.CommandType = CommandType.StoredProcedure;
            SqlParameter p = com.Parameters.Add("@ProyectoID", SqlDbType.Int);
            p.Value = Convert.ToInt32(ListaProyecto.SelectedValue.ToString());


            SqlParameter result = new SqlParameter("@retorno", SqlDbType.VarChar, 100);
            result.Direction = ParameterDirection.Output;
            com.Parameters.Add(result);



            conexion.Open();
            com.ExecuteNonQuery();
            url = Convert.ToString(result.SqlValue.ToString());
            conexion.Close();

            return url;
        }

        private void ListaProyecto_SelectedIndexChanged(object sender, EventArgs e)
        {

        }//interaccion con reporte

        private void lblAyuda_Click(object sender, EventArgs e)
        {

        }

        public bool CrearObtenerCSV(string pathDestino, string nombreNuevoArchivoPDF, out string mensaje)
        {
            try
            {
                nombreNuevoArchivoPDF = "PDF_" + nombreNuevoArchivoPDF;
                int contador = 0;

                string rutaArchivo = pathDestino + "\\" + nombreNuevoArchivoPDF + ".csv";
                while (File.Exists(rutaArchivo))
                {
                    rutaArchivo = pathDestino + "\\" + nombreNuevoArchivoPDF + " " + (contador + 1) + ".csv";
                    contador++;
                }
                //creo el archivo csv
                crearArchivo(rutaArchivo);
                EscribirMensajeDocumento("OrdenTrabajo", "Numero Control", "Comentario");
                mensaje = (nombreNuevoArchivoPDF + " " + (contador == 0 ? "" : contador.ToString())).Trim();
                return true;
            }
            catch (Exception ex)
            {
                mensaje = ex.Message;
                return false;
            }

        }

        public bool crearArchivo(string path)
        {
            try
            {
                DocumentoActual = System.IO.File.Create(path);
                return true;
            }
            catch (Exception ex)
            {
                return false;

                throw;
            }
        }

        public bool EscribirMensajeDocumento(string ordenTrabajo, string numeroControl, string comentario)
        {
            try
            {
                using (StringWriter sw = new StringWriter())
                {
                    sw.WriteLine(ordenTrabajo + "," + numeroControl + "," + comentario);
                    byte[] info = new UTF8Encoding(true).GetBytes(sw.ToString());
                    this.DocumentoActual.Write(info, 0, info.Length);
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool CerrarDocumento()
        {
            try
            {
                this.DocumentoActual.Close();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private void cmbFormatoSalida_SelectedIndexChanged(object sender, EventArgs e)
        {            
        }
    }

    public class Proyecto
    {
        public int ProyectoID { get; set; }
        public string Nombre { get; set; }
        public string RutaEspaniol { get; set; }

        public Proyecto()
        {
            ProyectoID = 0;
            Nombre = "--Selecciona un proyecto--";
            RutaEspaniol = "";
        }
    }
    public class FormatoClass
    {
        public int FormatoID { get; set; }
        public string Formato { get; set; }
    }
}