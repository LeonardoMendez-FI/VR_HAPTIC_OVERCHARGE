#include <WiFi.h>
#include <WiFiUdp.h>

const char* AP_SSID     = "RobotHaptic";
const char* AP_PASSWORD = "12345678";
const char* BROADCAST_IP = "192.168.4.255";
const uint16_t UNITY_PORT = 5005;
const uint16_t LOCAL_PORT  = 4444;
const uint32_t SEND_MS     = 30;

const uint8_t PIN_JL_X = 32;
const uint8_t PIN_JL_Y = 33;
const uint8_t PIN_JR_X = 34;
const uint8_t PIN_JR_Y = 35;
const uint8_t PIN_BTN_L = 25;
const uint8_t PIN_BTN_R = 26;

// Centros calibrados (reposo)
const uint16_t JL_X_CENTER = 1908;
const uint16_t JL_Y_CENTER = 1922;
const uint16_t JR_X_CENTER = 1937;
const uint16_t JR_Y_CENTER = 1914;
const uint16_t ADC_DEADZONE = 400;

WiFiUDP udp;
uint32_t ultimoEnvio = 0;

// Variables para estiramiento de pulso de botones
int pulseCountL = 0;
int pulseCountR = 0;
bool lastBtnL = HIGH;
bool lastBtnR = HIGH;

void setup() {
  Serial.begin(115200);
  delay(200);
  
  pinMode(PIN_BTN_L, INPUT_PULLUP);
  pinMode(PIN_BTN_R, INPUT_PULLUP);
  
  WiFi.mode(WIFI_AP);
  WiFi.softAP(AP_SSID, AP_PASSWORD);
  Serial.println("[AP] Creado. IP: " + WiFi.softAPIP().toString());
  
  udp.begin(LOCAL_PORT);
}

void loop() {
  if (millis() - ultimoEnvio >= SEND_MS) {
    ultimoEnvio = millis();
    enviarDatos();
    debugSerial();
  }
}

int8_t digitalizar(uint16_t valor, uint16_t centro) {
  if (valor < centro - ADC_DEADZONE) return -1;
  if (valor > centro + ADC_DEADZONE) return  1;
  return 0;
}

void enviarDatos() {
  int8_t jlx = digitalizar(analogRead(PIN_JL_X), JL_X_CENTER);
  int8_t jly = digitalizar(analogRead(PIN_JL_Y), JL_Y_CENTER);
  int8_t jrx = digitalizar(analogRead(PIN_JR_X), JR_X_CENTER);
  int8_t jry = digitalizar(analogRead(PIN_JR_Y), JR_Y_CENTER);
  
  // Leer estado actual de los botones
  bool btnL = digitalRead(PIN_BTN_L);
  bool btnR = digitalRead(PIN_BTN_R);
  
  // Si se detecta flanco de bajada (HIGH → LOW), activar pulso durante 2 envíos
  if (lastBtnL == HIGH && btnL == LOW) pulseCountL = 2;
  if (lastBtnR == HIGH && btnR == LOW) pulseCountR = 2;
  
  lastBtnL = btnL;
  lastBtnR = btnR;
  
  // Estado mantenido de los botones: 1 mientras esté pulsado
  uint8_t bL = (btnL == LOW) ? 1 : 0;
  uint8_t bR = (btnR == LOW) ? 1 : 0;
  
  // Si hay pulso estirado activo, lo mantenemos en 1 aunque ya se haya soltado
  if (pulseCountL > 0) bL = 1;
  if (pulseCountR > 0) bR = 1;
  
  if (pulseCountL > 0) pulseCountL--;
  if (pulseCountR > 0) pulseCountR--;

  // Paquete de 6 campos: X1,Y1,X2,Y2,B1,B2
  char paquete[32];
  snprintf(paquete, sizeof(paquete), "%d,%d,%d,%d,%d,%d", jly, jlx, jry, jrx, bL, bR);
  
  udp.beginPacket(BROADCAST_IP, UNITY_PORT);
  udp.write((uint8_t*)paquete, strlen(paquete));
  udp.endPacket();
}

void debugSerial() {
  Serial.printf("JL:(%d,%d) JR:(%d,%d) BTN: L=%d R=%d (pulse: %d,%d)\n",
    digitalizar(analogRead(PIN_JL_X), JL_X_CENTER),
    digitalizar(analogRead(PIN_JL_Y), JL_Y_CENTER),
    digitalizar(analogRead(PIN_JR_X), JR_X_CENTER),
    digitalizar(analogRead(PIN_JR_Y), JR_Y_CENTER),
    (digitalRead(PIN_BTN_L) == LOW) ? 1 : 0,
    (digitalRead(PIN_BTN_R) == LOW) ? 1 : 0,
    pulseCountL, pulseCountR
  );
}