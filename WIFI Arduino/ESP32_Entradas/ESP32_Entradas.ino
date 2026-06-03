#include <WiFi.h>
#include <WiFiUdp.h>

// ─────────────────── CONFIGURACIÓN DE RED (AP) ──────────────
const char* AP_SSID     = "RobotHaptic";
const char* AP_PASSWORD = "12345678";
const char* BROADCAST_IP = "192.168.4.255";
const uint16_t UNITY_PORT = 5005;
const uint16_t LOCAL_PORT  = 4444;
const uint32_t SEND_MS     = 30;

// ─────────────── PINES ORIGINALES (asumes resistencias externas) ─
#define PIN_JL_UP     4
#define PIN_JL_DOWN   13
#define PIN_JL_LEFT   39
#define PIN_JL_RIGHT  14

#define PIN_JR_UP     33
#define PIN_JR_DOWN   34
#define PIN_JR_LEFT   35
#define PIN_JR_RIGHT  36

#define PIN_BTN_L     27
#define PIN_BTN_R     5

// ──────────────────── VARIABLES GLOBALES ─────────────────────
WiFiUDP udp;
uint32_t ultimoEnvio = 0;

void setup() {
  Serial.begin(115200);
  delay(200);
  Serial.println("[ESP32 ENTRADAS] Iniciando Access Point...");

  pinMode(PIN_JL_UP,    INPUT);
  pinMode(PIN_JL_DOWN,  INPUT);
  pinMode(PIN_JL_LEFT,  INPUT);
  pinMode(PIN_JL_RIGHT, INPUT);

  pinMode(PIN_JR_UP,    INPUT);
  pinMode(PIN_JR_DOWN,  INPUT);
  pinMode(PIN_JR_LEFT,  INPUT);
  pinMode(PIN_JR_RIGHT, INPUT);

  pinMode(PIN_BTN_L, INPUT_PULLUP);
  pinMode(PIN_BTN_R, INPUT_PULLUP);

  WiFi.mode(WIFI_AP);
  if (WiFi.softAP(AP_SSID, AP_PASSWORD)) {
    Serial.println("[AP] Red creada: " + String(AP_SSID));
    Serial.println("[AP] IP: " + WiFi.softAPIP().toString());
  } else {
    Serial.println("[ERROR] No se pudo crear el AP.");
    ESP.restart();
  }

  udp.begin(LOCAL_PORT);
  Serial.println("[UDP] Listo. Enviando a " + String(BROADCAST_IP) + ":" + String(UNITY_PORT));
}

void loop() {
  if (millis() - ultimoEnvio >= SEND_MS) {
    ultimoEnvio = millis();
    enviarDatos();
    debugSerial();   // <-- debug activado
  }
}

int8_t leerEje(uint8_t pinPos, uint8_t pinNeg) {
  bool pos = digitalRead(pinPos) == HIGH;
  bool neg = digitalRead(pinNeg) == HIGH;
  if (pos && !neg) return 1;
  if (!pos && neg) return -1;
  return 0;
}

void enviarDatos() {
  int8_t jlx = leerEje(PIN_JL_RIGHT, PIN_JL_LEFT);
  int8_t jly = leerEje(PIN_JL_UP,    PIN_JL_DOWN);
  int8_t jrx = leerEje(PIN_JR_RIGHT, PIN_JR_LEFT);
  int8_t jry = leerEje(PIN_JR_UP,    PIN_JR_DOWN);

  uint8_t bL = (digitalRead(PIN_BTN_L) == LOW) ? 1 : 0;
  uint8_t bR = (digitalRead(PIN_BTN_R) == LOW) ? 1 : 0;

  char paquete[32];
  snprintf(paquete, sizeof(paquete), "%d,%d,%d,%d,%d,%d", jlx, jly, jrx, jry, bL, bR);

  udp.beginPacket(BROADCAST_IP, UNITY_PORT);
  udp.write((uint8_t*)paquete, strlen(paquete));
  udp.endPacket();
}

void debugSerial() {
  int8_t jlx = leerEje(PIN_JL_RIGHT, PIN_JL_LEFT);
  int8_t jly = leerEje(PIN_JL_UP,    PIN_JL_DOWN);
  int8_t jrx = leerEje(PIN_JR_RIGHT, PIN_JR_LEFT);
  int8_t jry = leerEje(PIN_JR_UP,    PIN_JR_DOWN);
  bool bL = digitalRead(PIN_BTN_L) == LOW;
  bool bR = digitalRead(PIN_BTN_R) == LOW;

  Serial.printf("JL:(%d,%d) JR:(%d,%d) BTN: L=%d R=%d\n", jlx, jly, jrx, jry, bL, bR);
}