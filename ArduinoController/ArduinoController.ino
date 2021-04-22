#include <Servo.h>

Servo Vertical;
Servo Horizontal;

float Vertical_input = 0.0;
int Vert_neutral_pos = 1470;
int Vert_max_pos = 250;//1200-1800;

float Horizontal_input = 0.0;
int Horiz_neutral_pos = 1600;
int Horiz_max_pos = 250;//1200-1800;


const byte buffSize = 5;
byte inputBuffer[buffSize];
const byte startMarker = 254;
const byte endMarker = 255;
byte bytesRecvd = 0;
boolean readInProgress = false;
//boolean newDataFromPC = false;


void setup() {
  Serial.begin(38400);

  Vertical.attach(9); // (pin, min pulse width, max pulse width in microseconds)
  Horizontal.attach(10); // (pin, min pulse width, max pulse width in microseconds)

  Horizontal.writeMicroseconds(Horiz_neutral_pos);
  Vertical.writeMicroseconds(Vert_neutral_pos);


  //starting_sequence();



}
void loop()
{
  
  
  received_data();
  if (!readInProgress)
  {
    calculate_input_vector();
    move_servo();

    // For Calibration
  //String s = "";
  //s.concat(String(Horizontal.readMicroseconds()));
  //s.concat(" ");
  //s.concat(String(Vertical.readMicroseconds()));
  //Serial.println(s);
  }

  

}

void received_data()
{
  if (Serial.available() > 0) {

    byte x = Serial.read();              //read char from serial

    if (x == endMarker) {                //look for end marker
      readInProgress = false;            //if found, set read in progress true (will stop adding new byte to buffer)
      //newDataFromPC = true;              //let arduino know that new data is available
      inputBuffer[bytesRecvd] = 0;       //clear input buffer
      processData();                      // process data in buffer
    }

    if (readInProgress) {
      inputBuffer[bytesRecvd] = x;      //populate input buffer with bytes
      bytesRecvd ++;                    //increment index
      if (bytesRecvd == buffSize) {     //when buffer is full
        bytesRecvd = buffSize - 1;      //keep space for end marker
      }
    }

    if (x == startMarker) {              // look for start maker
      bytesRecvd = 0;                    // if found, set byte received to 0
      readInProgress = true;             // set read in progress true
    }
  }
}

void processData() // for data type "<float, float, int>"
{
  int V_direction = 1;
  int H_direction = 1;
  
  if(inputBuffer[0] == 0){V_direction = -1;}
  if(inputBuffer[2] == 0){H_direction = -1;}
  Vertical_input = float(inputBuffer[1])/253.0 * V_direction;
  Horizontal_input = float(inputBuffer[3])/253.0 * H_direction;

}

void calculate_input_vector()
{
  float distance = sqrt(sq(Horizontal_input) + sq(Vertical_input));

  if (distance > 1)
  {
    Horizontal_input = Horizontal_input / distance;
    Vertical_input = Vertical_input / distance;
  }
}

void move_servo()
{
  Vertical.writeMicroseconds(Vertical_input * Vert_max_pos + Vert_neutral_pos);
  Horizontal.writeMicroseconds(Horizontal_input * Horiz_max_pos + Horiz_neutral_pos);
}

void starting_sequence()
{
  int delay_time = 1000;
  Vertical_input = 0.0;
  Horizontal_input = 0.0;
  move_servo();

  delay(delay_time);
  Vertical_input = 1.0;
  Horizontal_input = 0.0;
  move_servo();

  delay(delay_time);
  Vertical_input = -1.0;
  Horizontal_input = 0.0;
  move_servo();
  delay(delay_time);

  Vertical_input = 0.0;
  Horizontal_input = 0.0;
  move_servo();
  delay(delay_time);

  Horizontal_input = 1.0;
  Vertical_input = 0.0;
  move_servo();
  delay(delay_time);

  Horizontal_input = -1.0;
  Vertical_input = 0.0;
  move_servo();
  delay(delay_time);

  Vertical_input = 0.0;
  Horizontal_input = 0.0;
  move_servo();


  for (int deg = 0; deg <= 720; deg++) {
    float rad = (float(deg) * 71.0) / 4068.0;
    Vertical_input = cos(rad);
    Horizontal_input = sin(rad);
    move_servo();
    delay(2);
  }
  delay(20);

  for (int deg = 720; deg >= 0; deg--) {
    float rad = (float(deg) * 71.0) / 4068.0;
    Vertical_input = cos(rad);
    Horizontal_input = sin(rad);
    move_servo();
    delay(2);
  }

  delay(20);

  Vertical_input = 0.0;
  Horizontal_input = 0.0;
  move_servo();
}
