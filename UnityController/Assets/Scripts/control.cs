using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using System;


public class control : MonoBehaviour
{
  public bool is_controlling_UI = false;
  public bool is_stack_training = false;
  public GameObject platform;
  public GameObject ball;
  public GameObject target;

  float max_H_angle = 40f;
  float max_V_angle = 40f;

  float input_vertical = 1f;
  float input_horizontal = 1f;
  float previous_input_vertical = 0f;
  float previous_input_horizontal = 0f;

  float score = 0f;
  float hi_score = 0f;
  SerialPort Serial_port;

  // Initialization
  void Start()
  {
    Serial_port = new SerialPort("COM9", 38400);


  }



  //public Text axis_1;
  //public Text axis_2;
  //public Text game_text;
  //public Text score_text;
  //public Text target_distance;


  private int game_count = 0;



  bool horCalib = true;
  bool vertCalib = true;

  private void FixedUpdate()
  {
    input_horizontal = -Mathf.Clamp(Input.GetAxis("Horizontal"), -1f, 1f);
    input_vertical = Mathf.Clamp(Input.GetAxis("Vertical"), -1f, 1f);
    serial_output(input_horizontal, input_vertical);

    Debug.Log(Input.GetAxis("HorizontalR") + " " + Input.GetAxis("VerticalR"));
    if (!horCalib)
    {

      float _fValue = Input.GetAxis("HorizontalR");
      float _fEpsilon = 0.01f;

      if (((_fValue < _fEpsilon) && (_fValue > -_fEpsilon)))
      {
        horCalib = true;
        Serial_port.Open();
        for (int i = 0; i < 1000; ++i)
        {
          string value = Serial_port.ReadLine();
          Debug.Log(value);
        }
        Serial_port.Close();
      }

      input_horizontal = Mathf.Clamp(input_horizontal, -1f, 1f);
      input_horizontal -= 0.001f;
      serial_output(input_horizontal, 0);
      if (input_horizontal == -1f)
      {
        input_horizontal = 1f;
      }
      else if (input_horizontal == 1f)
      {
        input_horizontal = -1f;
      }

      //Debug.Log(Input.GetAxis("HorizontalR") + " " + Input.GetAxis("VerticalR"));








    }
    else if (!vertCalib)
    {
      float _fValue = Input.GetAxis("VerticalR");
      float _fEpsilon = 0.01f;

      if (((_fValue < _fEpsilon) && (_fValue > -_fEpsilon)))
      {
        vertCalib = true;

        Serial_port.Open();
        for (int i = 0; i < 1000; ++i)
        {
          string value = Serial_port.ReadLine();
          Debug.Log(value);
        }
        Serial_port.Close();

      }

      input_vertical = Mathf.Clamp(input_vertical, -1f, 1f);
      input_vertical -= 0.001f;
      serial_output(0, input_vertical);

      if (input_vertical == -1f)
      {
        input_vertical = 1f;
      }
      else if (input_vertical == 1f)
      {
        input_vertical = -1f;
      }


      //Debug.Log(Input.GetAxis("HorizontalR") + " " + Input.GetAxis("VerticalR"));
    }


  }

  // Executed each frame
  void Update()
  {


  }


  void serial_output(float H_input, float V_input)
  {
    byte[] message_to_send = new byte[6];   //byte array to send to arduino
    message_to_send[0] = 254;   //start marker 
    message_to_send[5] = 255;   //end marker

    int converted_V_value = Mathf.Abs((int)(V_input * 253));  //convert input in byte
    byte V_value = (byte)converted_V_value;

    int converted_H_value = Mathf.Abs((int)(H_input * 253));  //convert input in byte
    byte H_value = (byte)converted_H_value;

    if (input_vertical < 0) { message_to_send[1] = 0; }  //this byte is used to defined if Vinput in + or -
    else { message_to_send[1] = 1; }
    message_to_send[2] = V_value;

    if (input_horizontal < 0) { message_to_send[3] = 0; }  //this byte is used to defined if Hinput in + or -
    else { message_to_send[3] = 1; }
    message_to_send[4] = H_value;

    string to_print = message_to_send[0].ToString() + " - " +  //for debug
        message_to_send[1].ToString() + " - " +
        message_to_send[2].ToString() + " - " +
        message_to_send[3].ToString() + " - " +
        message_to_send[4].ToString() + " - " +
        message_to_send[5].ToString();
    //Debug.Log(to_print);

    //serialController.SendSerialMessage(message_to_send);

    Serial_port.Open();
    Serial_port.Write(message_to_send, 0, message_to_send.Length); //send message to arduino
    Serial_port.Close();
  }
}
