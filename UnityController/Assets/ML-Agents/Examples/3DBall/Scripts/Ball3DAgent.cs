using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Random = UnityEngine.Random;
using System.IO.Ports;


public class Ball3DAgent : Agent
{
  [Header("Specific to Ball3D")]
  public GameObject ball;
  [Tooltip("Whether to use vector observation. This option should be checked " +
      "in 3DBall scene, and unchecked in Visual3DBall scene. ")]
  public bool useVecObs;
  Rigidbody m_BallRb;
  EnvironmentParameters m_ResetParams;

  SerialPort Serial_port;
  float input_vertical = 0f;
  float input_horizontal = 0f;


  public override void Initialize()
  {
    Serial_port = new SerialPort("COM9", 38400);
    m_BallRb = ball.GetComponent<Rigidbody>();
    m_ResetParams = Academy.Instance.EnvironmentParameters;
    SetResetParameters();
  }

  public override void CollectObservations(VectorSensor sensor)
  {
    if (useVecObs)
    {
      sensor.AddObservation(gameObject.transform.rotation.z);
      sensor.AddObservation(gameObject.transform.rotation.x);
      sensor.AddObservation(ball.transform.position - gameObject.transform.position);
      sensor.AddObservation(m_BallRb.velocity);
    }
  }

  public override void OnActionReceived(ActionBuffers actionBuffers)
  {
    var actionZ = 2f * Mathf.Clamp(actionBuffers.ContinuousActions[0], -1f, 1f);
    var actionX = 2f * Mathf.Clamp(actionBuffers.ContinuousActions[1], -1f, 1f);

    input_horizontal = actionZ;
    input_vertical = actionX;
    serial_output(input_horizontal, input_vertical);

    if ((gameObject.transform.rotation.z < 0.25f && actionZ > 0f) ||
        (gameObject.transform.rotation.z > -0.25f && actionZ < 0f))
    {
      gameObject.transform.Rotate(new Vector3(0, 0, 1), actionZ);
    }

    if ((gameObject.transform.rotation.x < 0.25f && actionX > 0f) ||
        (gameObject.transform.rotation.x > -0.25f && actionX < 0f))
    {
      gameObject.transform.Rotate(new Vector3(1, 0, 0), actionX);
    }
    if ((ball.transform.position.y - gameObject.transform.position.y) < -2f ||
        Mathf.Abs(ball.transform.position.x - gameObject.transform.position.x) > 3f ||
        Mathf.Abs(ball.transform.position.z - gameObject.transform.position.z) > 3f)
    {
      SetReward(-1f);
      EndEpisode();
    }
    else
    {
      SetReward(0.1f);
    }
  }

  public override void OnEpisodeBegin()
  {
    gameObject.transform.rotation = new Quaternion(0f, 0f, 0f, 0f);
    gameObject.transform.Rotate(new Vector3(1, 0, 0), Random.Range(-10f, 10f));
    gameObject.transform.Rotate(new Vector3(0, 0, 1), Random.Range(-10f, 10f));
    m_BallRb.velocity = new Vector3(0f, 0f, 0f);
    ball.transform.position = new Vector3(Random.Range(-1.5f, 1.5f), 4f, Random.Range(-1.5f, 1.5f))
        + gameObject.transform.position;
    //Reset the parameters when the Agent is reset.
    SetResetParameters();
  }

  public override void Heuristic(in ActionBuffers actionsOut)
  {
    var continuousActionsOut = actionsOut.ContinuousActions;
    continuousActionsOut[0] = -Input.GetAxis("Horizontal");
    continuousActionsOut[1] = Input.GetAxis("Vertical");
  }

  public void SetBall()
  {
    //Set the attributes of the ball by fetching the information from the academy
    m_BallRb.mass = m_ResetParams.GetWithDefault("mass", 1.0f);
    var scale = m_ResetParams.GetWithDefault("scale", 1.0f);
    ball.transform.localScale = new Vector3(scale, scale, scale);
  }

  public void SetResetParameters()
  {
    SetBall();
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
