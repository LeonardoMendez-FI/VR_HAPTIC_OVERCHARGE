#include <WiFi.h>
#include <WiFiUdp.h>

const char* AP_SSID     = "RobotHaptic";
const char* AP_PASSWORD = "12345678";
const char* BROADCAST_IP = "192.168.4.255";
const uint16_t UNITY_PORT = 5005;
const uint16_t LOCAL_PORT  = 4444;
const uint32_t SEND_MS     = 100;  // más lento para leer mejor

const uint8_t PIN_JL_X = 32;
const uint8_t PIN_JL_Y = 33;
const uint8_t PIN_JR_X = 34;
const uint8_t PIN_JR_Y = 35;
const uint8_t PIN_BTN_L = 25;
const uint8_t PIN_BTN_R = 26;

WiFiUDP udp;
uint32_t ultimoEnvio = 0;

void setup() {
  Serial.begin(115200);
  delay(500);
  
  // Solo para diagnóstico: NO configuramos WiFi para no complicar
  pinMode(PIN_BTN_L, INPUT_PULLUP);
  pinMode(PIN_BTN_R, INPUT_PULLUP);
  
  Serial.println("DIAGNÓSTICO DE JOYSTICKS - Mueve las palancas y observa los valores");
  Serial.println("Valores crudos (0-4095):");
  Serial.println("JL_X\tJL_Y\tJR_X\tJR_Y\tBTN_L\tBTN_R");
}

void loop() {
  if (millis() - ultimoEnvio >= SEND_MS) {
    ultimoEnvio = millis();
    
    uint16_t jlx = analogRead(PIN_JL_X);
    uint16_t jly = analogRead(PIN_JL_Y);
    uint16_t jrx = analogRead(PIN_JR_X);
    uint16_t jry = analogRead(PIN_JR_Y);
    bool bL = digitalRead(PIN_BTN_L) == LOW;
    bool bR = digitalRead(PIN_BTN_R) == LOW;
    
    Serial.printf("%d\t%d\t%d\t%d\t%d\t%d\n", jlx, jly, jrx, jry, bL, bR);
  }
}