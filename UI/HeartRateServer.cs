using System;
using System.Net;
using System.IO;
using System.Text;
using System.Threading;
using System.Web;
using System.ComponentModel;

namespace HRServer
{
    public class HeartRateServer
    {
        HttpListener HttpListener { get; set; }

        public HeartRateServer(HttpListener httpListener)
        {
            HttpListener = httpListener;
        }



        public HttpListener _server;
        public int _port = 6547;

        public string _webRoot = Directory.GetCurrentDirectory() + @"/www/";

        public string _lastUpdate = "hh:mm:ss";
        public bool _isServerStarted = false;

        public string _bpm = "000";

        public event PropertyChangedEventHandler PropertyChanged;


        public bool IsServerStarted
        {
            get { return _isServerStarted; }
            set { _isServerStarted = value; }
        }

        public string LastUpdate
        {
            get { return _lastUpdate; }
            set 
            {
                _lastUpdate = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("LastUpdate"));
            }
        }

        public string BPM
        {
            get { return _bpm; }
            set 
            { 
                _bpm = value;
                LastUpdate = DateTime.Now.ToString("HH:mm:ss");
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("BPM"));
            }
        }

        //public HttpListener Server
        //{
        //    get { return _server; }
        //    private set { _server = value; }
        //}

        public string WebRoot
        {
            get { return _webRoot; }
        }

        public int Port
        {
            get { return _port; }
            set { 
                if (value > 65535 || value <= 0) {
                    _port = 6547;
                } else{
                    _port = value;
                }
            }
        }

        public void StartServer()
        {
            this.StartListener();
            this.WaitNextRequest();
            IsServerStarted = true;

        }

        private void WaitNextRequest()
        {

               IAsyncResult context = HttpListener.BeginGetContext(
                   new AsyncCallback(ListenerCallBack), HttpListener);
            
            
        }

        public void StopServer()
        {
            this.StopListener();
            HttpListener.Stop();
            IsServerStarted = false;


            //

            HttpListener = new HttpListener();  // this is the http server
            string hostName = Dns.GetHostName();

            HttpListener.Prefixes.Add("http://127.0.0.1:" + Port + "/");  //we set a listening address here (localhost)
            HttpListener.Prefixes.Add("http://localhost:" + Port + "/");  //we set a listening address here (localhost)

            foreach (IPAddress ip in Dns.GetHostEntry(hostName).AddressList)
            {
                if (ip.AddressFamily.ToString() == "InterNetwork")
                {
                    HttpListener.Prefixes.Add("http://" + ip + ":" + Port + "/");  //we set a listening address here (localhost)
                }
            }

        }

        private void ListenerCallBack(IAsyncResult result)
        {
            HttpListener listener = (HttpListener)result.AsyncState;

            HttpListenerContext context;
            try
            {
                context = listener.EndGetContext(result);
            }
            catch (Exception exp)
            {
                return; 
            } 

            HttpListenerResponse response = context.Response;

            string file = context.Request.Url.LocalPath;
            if (file == "/")
            {
                file = @"index.html";
            }

            string page = WebRoot + file;
            //this will get the page requested by the browser 

            String msg = "";
            try
            {
                if (context.Request.HttpMethod.Contains("GET"))
                {
                    msg = HandleGETRequest(page);
                }
                else if (context.Request.HttpMethod.Contains("POST"))
                {
                    msg = HandlePOSTRequest(context);
                }
                else
                {
                    throw new HttpListenerException(405, "Request method non supported.");
                }
            }
            catch (FileNotFoundException fnfe)
            {
                response.StatusCode = 404;
                Console.Out.WriteLine("/!/ 404 ! => " + fnfe.Message);

            }
            catch (Exception e)
            {
                Console.Out.WriteLine("Erreur ! => " + e.Message);
            }


            byte[] buffer = Encoding.UTF8.GetBytes(msg);
            //then we transform it into a byte array
            response.ContentLength64 = buffer.Length;  // set up the messasge's length
            Stream st = response.OutputStream;  // here we create a stream to send the message
            st.Write(buffer, 0, buffer.Length); // and this will send all the content to the browser
            context.Response.Close();  // here we close the connection

            this.WaitNextRequest();

        }

        private void StartListener()
        {
            HttpListener.Start();
        }

        private void StopListener()
        {
            HttpListener.Stop();
        }

        public string HandleGETRequest(String page) {

            TextReader tr = new StreamReader(page);
            return tr.ReadToEnd();  //getting the page's content

        }

        public string HandlePOSTRequest(HttpListenerContext context)
        {
            var data_text = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding).ReadToEnd();
            var clean = HttpUtility.ParseQueryString(data_text);

            string hr = clean.Get("rate");
            BPM = hr;
            if (!Directory.Exists(@"./www/"))
            {
                Directory.CreateDirectory(@"./www/");
            }
            using (StreamWriter file = new StreamWriter(@"./www/hr.txt", false))
            {
                file.WriteLine(hr);
            }

            return "OK"; // answer provided to the smart watch for an "ack"

        }

        

        


    }
}
