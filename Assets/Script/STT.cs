using UnityEngine;
using UnityEngine.UI;
using FrostweepGames.Plugins.GoogleCloud.SpeechRecognition;
using FrostweepGames.Plugins.GoogleCloud.TextToSpeech;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System;
using System.Collections;
using UnityEngine.SceneManagement;
public class STT : MonoBehaviour
{
    GameObject yuna;

    public string[] dic_keys;// = new string[lines.Length];
    public string[] dic_values;// = new string[lines.Length];

    private GCSpeechRecognition _speechRecognition;

    private Socket m_Socket;
    private string iPAdress = "127.0.0.1";
    private const int kPort1 = 51001;
    private int SenddataLength;
    public int ReceivedataLength;
    private byte[] Sendbyte;
    public byte[] Receivebyte = new byte[2000];
    private string transcript;
    public string ReceiveString;
    
    string encode_char(string key)
    {
        for (int i = 0; i < dic_keys.Length; i++)
        {
            if (dic_keys[i].Equals(key) == true)
            {
                return dic_values[i];
            }
        }
        return "False";
    }
    string encode_string(string sentence)
    {
        string result = "";
        string[] words = sentence.Split(' ');

        for (int w = 0; w < words.Length; w++)
        {
            char[] chars = words[w].ToCharArray();

            for (int i = 0; i < chars.Length; i++)
            {
                string a = encode_char(chars[i].ToString());

                if (a.Equals("False") == true)
                {
                    result += chars[i].ToString();
                }

                else
                {
                    result += a;
                }
                result += "##";
            }
            result += "##";
        }
        return result;
    }

    void Start()
    {
        yuna = GameObject.Find("yuna");
        TextAsset textAsset = Resources.Load<TextAsset>("dic_file");
        string[] lines = textAsset.text.Split('\n');
        dic_keys = new string[lines.Length - 1];
        dic_values = new string[lines.Length - 1];

        for (int i = 0; i < lines.Length - 1; i++)
        {
            string[] tokens = lines[i].Split('#');

            dic_keys[i] = tokens[0];
            dic_values[i] = tokens[1];
        }
        _speechRecognition = GCSpeechRecognition.Instance;
        _speechRecognition.RecognizeSuccessEvent += RecognizeSuccessEventHandler;
        _speechRecognition.RecognizeFailedEvent += RecognizeFailedEventHandler;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
        _speechRecognition.StartRecord(false);
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            _speechRecognition.StopRecord();
        }
    }
    
    private void RecognizeSuccessEventHandler(RecognitionResponse recognitionResponse)
    {
        if (recognitionResponse == null || recognitionResponse.results.Length == 0)
        {
            
        }

        else
        {
            transcript = recognitionResponse.results[0].alternatives[0].transcript;
            Debug.Log(transcript);
            
            if (transcript.Contains("안녕"))
            {
                ReceiveString = "안녕하세요";
            }
            else if(transcript.Contains("이름"))
            {
                ReceiveString = "제 이름은 유나입니다.";
            }
            else if(transcript.Contains("춤"))
            {
                ReceiveString = "춤을 추겠습니다.";
                yuna.GetComponent<DanceBehaviourScript>().dancing();
            }
        }
    }


    private void RecognizeFailedEventHandler(string error)
    {
        Debug.Log("recognized Fail" + error);
    }

    void Awake2()
    {
        // Socket create.
        m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        m_Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, 2500);

        // Socket connect.
        try
        {
            IPAddress ipAddr = System.Net.IPAddress.Parse(iPAdress);
            IPEndPoint ipEndPoint = new System.Net.IPEndPoint(ipAddr, kPort1);
            m_Socket.Connect(ipEndPoint);
        }
        catch (SocketException SCE)
        {
            Debug.Log("Socket connect error! : " + SCE.ToString());
            return;
        }

        transcript = '가' + transcript;
        string encoded_transcript = encode_string(transcript);
        StringBuilder sb = new StringBuilder();
        sb.Append(encoded_transcript);

        try
        {
            // Send.
            SenddataLength = Encoding.Default.GetByteCount(sb.ToString());
            Sendbyte = Encoding.Default.GetBytes(sb.ToString());
            m_Socket.Send(Sendbyte, Sendbyte.Length, 0);

            // Receive.
            m_Socket.Receive(Receivebyte);
            ReceiveString = Encoding.Default.GetString(Receivebyte);
            ReceivedataLength = Encoding.Default.GetByteCount(ReceiveString.ToString());
            Debug.Log(ReceiveString);
        }

        catch (SocketException err)
        {
            Debug.Log("Socket send or receive error! : " + err.ToString());
        }
    }

    void OnApplicationQuit()
    {
        try
        {
            m_Socket.Close();
            m_Socket = null;
        }

        catch (NullReferenceException err)
        {
            Debug.Log("Socket unopened : " + err.ToString());
        }
    }
}