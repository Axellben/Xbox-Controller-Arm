using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System.IO.Ports;

public class MoveToGoalAgent : Agent
{

  [SerializeField] private Transform targetTransform;

  float max_H_angle = 40f;
  float max_V_angle = 40f;

  float input_vertical = 0f;
  float input_horizontal = 0f;
  float previous_input_vertical = 0f;
  float previous_input_horizontal = 0f;

  SerialPort Serial_port;


  //public int target_FPS = 30;

  private void Start()
  {
    Serial_port = new SerialPort("COM9", 38400);
    //QualitySettings.vSyncCount = 0;
    //Application.targetFrameRate = target_FPS;
    //Screen.SetResolution(1920, 1080, true);
    //Screen.fullScreen = !Screen.fullScreen;

  }

  public override void OnEpisodeBegin()
  {
    transform.localPosition = new Vector3(Random.Range(-5f, 5f), 0, Random.Range(-5f, 5f));
    targetTransform.localPosition = new Vector3(Random.Range(-5f, 5f), 0.5f, Random.Range(-5f, 5f)); ;
  }

  public override void OnActionReceived(ActionBuffers actions)
  {
    //float moveX = actions.ContinuousActions[0];
    //float moveZ = actions.ContinuousActions[1];





    /*
    float distance = Vector2.Distance(new Vector2(targetTransform.localPosition.x, targetTransform.localPosition.z),
                                                  new Vector2(transform.localPosition.x, transform.localPosition.z));
    if (distance < 0.05) { distance = 0f; }


    distance = Mathf.Clamp(distance, 0f, 0.98f);
    float smoothness = Vector2.Distance(new Vector2(input_horizontal, input_vertical),
                                        new Vector2(previous_input_horizontal, previous_input_vertical));
    smoothness = Mathf.Clamp(smoothness, 0f, 1f);
    //if (smoothness < 0.015) { smoothness = 0f; }
    //Debug.Log("smoothness : " + smoothness);

    //float reward = (1 - distance)/100;
    //float reward = (1 - distance)/100 + (1-smoothness)/100;
    //float reward = Mathf.Pow((1-distance), 2F)/100 + smoothness_importance*(1 - distance)*Mathf.Pow((1 - smoothness), 2f)/200;
    //float reward = Mathf.Pow((1 - smoothness), 1.5f)/100 + Mathf.Pow((1 - distance), 2f)/100;//+(1 - distance)/100;
    //float reward = Mathf.Pow((1 - distance), 2f) / 100 + Mathf.Pow((1 - Mathf.Abs(input_horizontal)), 2f)/300 + Mathf.Pow((1 - Mathf.Abs(input_vertical)), 2f)/300+ Mathf.Pow((1 - smoothness), 2f) / 100;
    float reward = Mathf.Pow((1 - smoothness), 2.0f) / 100 + Mathf.Pow((1 - distance), 2.0f) / 100;//+(1 - distance)/100;
    SetReward(reward);



    previous_input_vertical = input_vertical;
    previous_input_horizontal = input_horizontal;
    */

    input_horizontal = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);    //replace by vector action
    input_vertical = Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f);    //replace by vector action
    serial_output(-input_horizontal, input_vertical);

    float moveSpeed = 6f;
    transform.localPosition += new Vector3(input_horizontal, 0, input_vertical) * Time.deltaTime * moveSpeed;

  }

  public override void CollectObservations(VectorSensor sensor)
  {
    sensor.AddObservation(transform.localPosition);
    sensor.AddObservation(targetTransform.localPosition);
  }

  public override void Heuristic(in ActionBuffers actionsOut)
  {
    ActionSegment<float> continuousAction = actionsOut.ContinuousActions;
    continuousAction[0] = Input.GetAxis("Horizontal");
    continuousAction[1] = Input.GetAxis("Vertical");

  }


  private void OnTriggerEnter(Collider other)
  {
    Debug.Log("Fuck");
    if (other.TryGetComponent<Goal>(out Goal goal))
    {
      SetReward(1f);
      EndEpisode();
    }
    if (other.TryGetComponent<Wall>(out Wall wall))
    {
      SetReward(-1f);
      EndEpisode();
    }
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
