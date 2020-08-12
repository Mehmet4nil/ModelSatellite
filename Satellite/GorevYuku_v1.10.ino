//-------------------------------LIBRARY------------------------------------
#include <Adafruit_GPS.h>
#include <SoftwareSerial.h>
#include "Seeed_BME280.h"
#include <Wire.h>
#include <SDL_Arduino_INA3221.h>

/*-------------------------------ACIKLAMA------------------------------------
  - GPS BAĞLANTISI | RX:3 TX:2  | 3v3
  - XBEE BAĞLANTISI | RX:4 TX:5 | 5V
  - BMP280 BAĞLANTISI | SDA:A4 SCL:A5 | 5V
  - VOLTAGE SENSOR BAĞLANTISI | SDA:A4 SCL:A5 | 3v3
  - SDCard BAĞLANTISI | 3v3
  - BUZZER BAĞLANTISI | 5V
  - SERVO MOTOR | KAHVE:GND KIRMIZI:5V TURUNCU:10(GÜÇ PİNİ) | 5V
*/

SoftwareSerial mySerial(6, 7);// TX:6 | RX:7 | 5V
SoftwareSerial Xbee(4, 5); //RX:5 | TX:4  | 3v3
SoftwareSerial cpu(8, 9); // TX:2 | RX:3


Adafruit_GPS GPS(&mySerial);
BME280 bme280;
SDL_Arduino_INA3221 ina3221;

int takimNo = 47801, paketNo = 0, satel, gun = 1, ay = 9, yil = 2019, saat = 13, dakika = 45, saniye = 11, donus = 0, minVal = 265, maxVal = 402;
float basinc, yukseklik, hiz, sicaklik, vin = -9.9, rakim = -9.9, rakim_before = -9.9, alt = -9.9, la = -9.9, lg = -9.9, min_rakim = 1010.0;
const int MPU_addr = 0x68; // I2C address of the MPU-6050
int16_t AcX, AcY, AcZ;
char komut = 'Q', i, j ;
String durum = "BEKLEMEDE";
bool dusme = false, ayrilma = false;
uint32_t timer = millis();
int x, y, z = 0, z_before, avarage, t_avarage;

void bme280_Read();
void voltage_Read();
void mpu6500_Read();
void komut_al();

void setup()  {
  pinMode (A2, OUTPUT) ;
  for (i = 0; i < 100; i++)
  {
    digitalWrite (A2, HIGH) ;
    delay (2) ;
    digitalWrite (A2, LOW) ;
    delay (2) ;
  }
  for (i = 0; i < 60; i++) // When a frequency sound
  {
    digitalWrite (A2, HIGH) ; //send tone
    delay (1) ;
    digitalWrite (A2, LOW) ; //no tone
    delay (1) ;
  }
  Serial.begin(9600);

  Serial.println( "Arduino started sending bytes via XBee" );
  // set the data rate for the SoftwareSerial port

  cpu.begin( 9600 );
  Xbee.begin( 9600 );
  GPS.begin(9600);



  delay( 5000 );

  Wire.begin();
  Wire.beginTransmission(MPU_addr);
  Wire.write(0x6B);  // PWR_MGMT_1 register
  Wire.write(0);     // set to zero (wakes up the MPU-6050)
  Wire.endTransmission(true);
  // 9600 NMEA is the default baud rate for Adafruit MTK GPS's- some use 4800

  if (!bme280.init()) {
    Serial.println("Device error!");
  }

  // uncomment this line to turn on RMC (recommended minimum) and GGA (fix data) including altitude
  GPS.sendCommand(PMTK_SET_NMEA_OUTPUT_RMCGGA);
  // uncomment this line to turn on only the "minimum recommended" data
  //GPS.sendCommand(PMTK_SET_NMEA_OUTPUT_RMCONLY);
  // For parsing data, we don't suggest using anything but either RMC only or RMC+GGA since
  // the parser doesn't care about other sentences at this time
  // Set the update rate
  GPS.sendCommand(PMTK_SET_NMEA_UPDATE_1HZ);   // 1 Hz update rate
  // For the parsing code to work nicely and have time to sort thru the data, and
  // print it out we don't suggest using anything higher than 1 Hz
  // Request updates on antenna status, comment out to keep quiet
  GPS.sendCommand(PGCMD_ANTENNA);


  delay(1000);
  // Ask for firmware version
  mySerial.println(PMTK_Q_RELEASE);
}

//-------------------------------LOOP------------------------------------

void loop()  {

  char c = GPS.read();

  // if you want to debug, this is a good time to do it!
  // if a sentence is received, we can check the checksum, parse it...
  if (GPS.newNMEAreceived()) {
    // a tricky thing here is if we print the NMEA sentence, or data
    // we end up not listening and catching other sentences!
    // so be very wary if using OUTPUT_ALLDATA and trytng to print out data
    //Serial.println(GPS.lastNMEA());   // this also sets the newNMEAreceived() flag to false

    if (!GPS.parse(GPS.lastNMEA()))   // this also sets the newNMEAreceived() flag to false
      return;  // we can fail to parse a sentence in which case we should just wait for another
  }
  if (timer > millis()) timer = millis();

  if (millis() - timer > 1000) {
    timer = millis(); // reset the timer
    bme280_Read();
    mpu6500_Read();
    voltage_Read();
    komut_al();


    //    Serial.println(komut);
    //    cpu.print(komut);

    gun = GPS.day;
    ay = GPS.month;
    yil = GPS.year;
    saat = GPS.hour;
    dakika = GPS.minute;
    saniye = GPS.seconds;
    paketNo++;
    Xbee.print(paketNo);
    Xbee.print(',');
    Xbee.print(gun);
    Xbee.print(',');
    Xbee.print(ay);
    Xbee.print(',');
    Xbee.print(yil);
    Xbee.print(",");
    Xbee.print(saat + 3);
    Xbee.print(',');
    Xbee.print(dakika);
    Xbee.print(',');
    Xbee.print(saniye);
    Xbee.print(",");
    Xbee.print(basinc);
    Xbee.print(",");
    Xbee.print(yukseklik);
    Xbee.print(",");
    Xbee.print(hiz);
    Xbee.print(",");
    Xbee.print(sicaklik);
    Xbee.print(",");
    Xbee.print(vin);
    Xbee.print(",");
    la = GPS.latitudeDegrees;
    lg = GPS.longitudeDegrees;
    alt = GPS.altitude;
    Xbee.print(la, 4);
    Xbee.print(",");
    Xbee.print(lg, 4);
    Xbee.print(",");
    Xbee.print(alt, 1);

    Xbee.print(",");
    Xbee.print(durum);
    Xbee.print(",");
    Xbee.print(x - 180);
    Xbee.print(",");
    Xbee.print(y - 180);
    Xbee.print(",");
    Xbee.print(z);
    Xbee.print(",");
    Xbee.print(donus);
    Xbee.println(",");

    //delay(900);

    if (dusme == true) {
      if (yukseklik <= 410) {
        if (ayrilma == true) {
          durum = "GOREV YUKU INIS";
          if (yukseklik <= 30) {
            //-------------buzzer------------
            for (i = 0; i < 80; i++) // When a frequency sound
            {
              digitalWrite (A2, HIGH) ; //send tone
              delay (1) ;
              digitalWrite (A2, LOW) ; //no tone
              delay (1) ;
            }
            for (i = 0; i < 100; i++)
            {
              digitalWrite (A2, HIGH) ;
              delay (2) ;
              digitalWrite (A2, LOW) ;
              delay (2) ;
            }
            //-------------buzzer------------
            durum = "KURTARMA";
          }
        }
        else {
          //--------ayrilma-------
          cpu.print('A');
          ayrilma = true;
          delay(200);
          cpu.print('A');
          durum = "AYRILMA";
        }
      }
    }
    else {
      if (yukseklik > 400) {
        dusme = true;

      }
    }
  }
}
//-------------------------------FUNCTIONS------------------------------------

void bme280_Read() {
  sicaklik = bme280.getTemperature();
  basinc = bme280.getPressure();
  rakim = bme280.calcAltitude(basinc);
  //------------YUKSEKLIK-------------
  yukseklik = rakim - min_rakim;
  if (yukseklik <= 0)
  {
    yukseklik = 0;
  }
  //------------HIZ-------------------
  hiz = rakim - rakim_before;
  rakim_before = rakim;
}

void mpu6500_Read() {
  Wire.beginTransmission(MPU_addr);
  Wire.write(0x3B);  // starting with register 0x3B (ACCEL_XOUT_H)
  Wire.endTransmission(false);
  Wire.requestFrom(MPU_addr, 14, true); // request a total of 14 registers
  AcX = Wire.read() << 8 | Wire.read(); // 0x3B (ACCEL_XOUT_H) & 0x3C (ACCEL_XOUT_L)
  AcY = Wire.read() << 8 | Wire.read(); // 0x3D (ACCEL_YOUT_H) & 0x3E (ACCEL_YOUT_L)
  AcZ = Wire.read() << 8 | Wire.read(); // 0x3F (ACCEL_ZOUT_H) & 0x40 (ACCEL_ZOUT_L)

  int xAng = map(AcX, minVal, maxVal, -90, 90);
  int yAng = map(AcY, minVal, maxVal, -90, 90);
  int zAng = map(AcZ, minVal, maxVal, -90, 90);

  x = RAD_TO_DEG * (atan2(-yAng, -zAng) + PI);
  y = RAD_TO_DEG * (atan2(-xAng, -zAng) + PI);
  z = RAD_TO_DEG * (atan2(-yAng, -xAng) + PI);

  avarage = z - z_before;
  avarage = abs(avarage);
  t_avarage += avarage;
  if (t_avarage >= 360) {
    t_avarage = 0;
    donus += 1;
  }
  z_before = z;

}

void voltage_Read() {
  vin = ina3221.getBusVoltage_V(1);
}
void komut_al() {
  if (Xbee.available() > 0) {
    komut = Xbee.read();
    if (komut == 'A' || komut == 'K') {
      cpu.print(komut);
      Serial.print(komut);
    }

    //delay(100);
  }
}
