using System.Collections.Generic;
using UnityEngine;
using FrostweepGames.Plugins.GoogleCloud.TextToSpeech;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Net;
using System.Text;
using System.IO;

public class TTS : MonoBehaviour
{
    private GCTextToSpeech _gcTextToSpeech;
    public Queue<AudioClip> audioQueue;
    public AudioSource audioSource;

    private readonly ManualResetEvent stoppeing_event_ = new ManualResetEvent(false);

    public string ReceiveString;
    public string msg_server;
    float num = 0;

    public string avatar_name;

    void Start()
    {
        _gcTextToSpeech = GCTextToSpeech.Instance;
        _gcTextToSpeech.SynthesizeSuccessEvent += _gcTextToSpeech_SynthesizeSuccessEvent;
        _gcTextToSpeech.SynthesizeFailedEvent += _gcTextToSpeech_SynthesizeFailedEvent;
        audioQueue = new Queue<AudioClip>();

        avatar_name = "noyj";
        TextToSpeech("안녕하세요. 만나서 반가워요.");
    }

    // Update is called once per frame
    void Update()
    {
        STT stt = GameObject.Find("TTSSTT").GetComponent<STT>();

        ReceiveString = "";
        msg_server = "";

        if (ReceiveString != stt.ReceiveString)
        {
            if (stt.ReceiveString.Length >= 1)
            {
                num += 1;
                Debug.Log(stt.ReceiveString);
                TextToSpeech(stt.ReceiveString);
                if (num >= 1)
                {
                    stt.ReceiveString = "";
                }
            }
        }
    }

    public void TextToSpeech(string sentence)
    {
        AudioSource audio = GetComponent<AudioSource>();

        string client_id = "9h1u7ourot";
        string client_secret = "QRHqvOu2yXdqGc2V96b7ixiY3ymUXRGOx7WQBZRw";

        string url = "https://naveropenapi.apigw.ntruss.com/tts-premium/v1/tts";
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
        request.Headers.Add("X-NCP-APIGW-API-KEY-ID", client_id);
        request.Headers.Add("X-NCP-APIGW-API-KEY", client_secret);
        request.Method = "POST";

        byte[] byteDataParams = Encoding.UTF8.GetBytes($"speaker={avatar_name}&volume=0&speed=0&pitch=0&format=wav&text={sentence}");

        request.ContentType = "application/x-www-form-urlencoded";
        request.ContentLength = byteDataParams.Length;

        Stream st = request.GetRequestStream();
        st.Write(byteDataParams, 0, byteDataParams.Length);
        st.Close();
        HttpWebResponse response = (HttpWebResponse)request.GetResponse();

        Stream input = response.GetResponseStream();

        var memoryStream = new MemoryStream();
        input.CopyTo(memoryStream);
        byte[] byteArray = memoryStream.ToArray();
        float[] f = ConvertByteToFloat(byteArray);

        using (Stream s = new MemoryStream(byteArray))
        {
            AudioClip audioClip = AudioClip.Create("tts123", f.Length, 1, 24000, false);
            audioClip.SetData(f, 0);
            audio.clip = audioClip;
            audio.Play();
        }
    }
    private float[] ConvertByteToFloat(byte[] array)
    {
        float[] floatArr = new float[array.Length / 2];
        for (int i = 0; i < floatArr.Length; i++)
        {
            floatArr[i] = BitConverter.ToInt16(array, i * 2) / 32768.0f;
        }
        return floatArr;
    }

    [DllImport("user32.dll")]
    public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

    [DllImport("user32")]
    public static extern int SetCursorPos(int x, int y);

    #region failed handlers

    private void _gcTextToSpeech_SynthesizeFailedEvent(string error)
    {
        //Debug.Log(error);
        //GCSpeechRecognition.instance.TextToSpeechWork();
    }

    #endregion failed handlers

    #region sucess handlers

    private void _gcTextToSpeech_SynthesizeSuccessEvent(PostSynthesizeResponse response)
    {
        try
        {
            AudioClip clip = _gcTextToSpeech.GetAudioClipFromBase64(response.audioContent, Constants.DEFAULT_AUDIO_ENCODING);
            audioSource.clip = clip;

            audioQueue.Enqueue(clip);
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
    }
    #endregion sucess handlers
}