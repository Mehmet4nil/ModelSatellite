/*
  Xbee1
  D. Thiebaut
  
  Makes Arduino send 1 character via XBee wireless to another XBee connected
  to a computer via a USB cable. 

  The circuit: 
  * RX is digital pin 2 (connect to TX of XBee)
  * TX is digital pin 3 (connect to RX of XBee)
 
  Based on a sketch created back in the mists of time by Tom Igoe
  itself based on Mikal Hart's example
 
*/
#include <Servo.h>

#include <SoftwareSerial.h>
Servo myservo;
SoftwareSerial xbee(5,4); // RX, TX
char c = 'Q', d;
int  pos = 0;    // variable to store the servo position


void setup()  {
   Serial.begin(9600);
   myservo.attach(9);
   // set the data rate for the SoftwareSerial port
   xbee.begin( 9600 );
}

void loop()  {
  if (xbee.available() > 0) {
    c = xbee.read();
  }
  Serial.println(c);

  // send character via XBee to other XBee connected to Mac
  // via USB cable

  if(c == 'A'){
      myservo.write(0);
      //delay(500);
      //digitalWrite(9, LOW);
      c = 'Q';
    }
  else if (c == 'K'){
      myservo.write(75);
      c = 'Q';
    }
  delay( 1000 );
}
