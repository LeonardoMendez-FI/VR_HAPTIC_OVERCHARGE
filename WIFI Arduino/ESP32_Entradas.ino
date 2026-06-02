#include <WiFi.h>
#include <WiFiUdp.h>

// ─────────────────── CONFIGURACIÓN DE RED (AP) ──────────────
const char* AP_SSID     = "RobotHaptic";
const char* AP_PASSWORD = "12345678";        // mínimo 8 caracteres
const char* BROADCAST_IP = "192.168.4.255";  // dirección de broadcast
const uint16_t UNITY_PORT = 5005;
const uint16_t LOCAL_PORT  = 4444;
const uint32_t SEND_MS     = 30;

// ─────────────── CONFIGURACIÓN DE PINES ─────────────────────
const uint8_t PIN_JL_X = 32;
const uint8_t PIN_JL_Y = 33;
const uint8_t PIN_JR_X = 34;
const uint8_t PIN_JR_Y = 35;
const uint8_t PIN_BTN_L = 25;   // botón joystick izquierdo
const uint8_t PIN_BTN_R = 26;   // botón joystick derecho

// ─────────────── UMBRALES PARA DIGITALIZAR ──────────────────
const uint16_t ADC_CENTER   = 2048;
const uint16_t ADC_DEADZONE = 300;

// ──────────────────── VARIABLES GLOBALES ─────────────────────
WiFiUDP udp;
uint32_t ultimoEnvio = 0;

void setup() {
  Serial.begin(115200);
  delay(200);
  Serial.println("[ESP32 ENTRADAS] Iniciando Access Point...");

  // Configurar pines de botones
  pinMode(PIN_BTN_L, INPUT_PULLUP);
  pinMode(PIN_BTN_R, INPUT_PULLUP);

  // Crear el Access Point
  WiFi.mode(WIFI_AP);
  bool apOk = WiFi.softAP(AP_SSID, AP_PASSWORD);
  if (apOk) {
    Serial.println("[AP] Red creada: " + String(AP_SSID));
    Serial.print("[AP] IP de esta placa: ");
    Serial.println(WiFi.softAPIP());               // será 192.168.4.1
  } else {
    Serial.println("[ERROR] No se pudo crear el AP. Reinicie.");
    ESP.restart();
  }

  udp.begin(LOCAL_PORT);
  Serial.print("[UDP] Enviando a broadcast ");
  Serial.print(BROADCAST_IP);
  Serial.print(":");
  Serial.println(UNITY_PORT);
}

void loop() {
  if (millis() - ultimoEnvio >= SEND_MS) {
    ultimoEnvio = millis();
    enviarDatos();
  }
}

int8_t digitalizar(uint16_t valor) {
  if (valor < ADC_CENTER - ADC_DEADZONE) return -1;
  if (valor > ADC_CENTER + ADC_DEADZONE) return  1;
  return 0;
}

void enviarDatos() {
  int8_t jlx = digitalizar(analogRead(PIN_JL_X));
  int8_t jly = digitalizar(analogRead(PIN_JL_Y));
  int8_t jrx = digitalizar(analogRead(PIN_JR_X));
  int8_t jry = digitalizar(analogRead(PIN_JR_Y));

  uint8_t bL = (digitalRead(PIN_BTN_L) == LOW) ? 1 : 0;
  uint8_t bR = (digitalRead(PIN_BTN_R) == LOW) ? 1 : 0;

  char paquete[32];
  snprintf(paquete, sizeof(paquete), "%d,%d,%d,%d,%d,%d", jlx, jly, jrx, jry, bL, bR);

  udp.beginPacket(BROADCAST_IP, UNITY_PORT);
  udp.write((uint8_t*)paquete, strlen(paquete));
  udp.endPacket();
}