#include <WiFi.h>
#include <WiFiUdp.h>

// ─────────────────── CONFIGURACIÓN DE RED ──────────────────
const char* AP_SSID     = "RobotHaptic";
const char* AP_PASSWORD = "12345678";
const char* STATIC_IP   = "192.168.4.2";    // IP cuando es cliente
const char* GATEWAY     = "192.168.4.1";
const char* SUBNET      = "255.255.255.0";
const uint16_t LOCAL_PORT = 5006;

// ─────────────── CONFIGURACIÓN DE PINES ─────────────────────
const uint8_t PIN_DANO_1  = 12;
const uint8_t PIN_DANO_2  = 13;
const uint8_t PIN_DANO_3  = 14;
const uint8_t PIN_DANO_4  = 27;
const uint8_t PIN_CARGA_1 = 15;
const uint8_t PIN_CARGA_2 = 16;
const uint8_t PIN_CARGA_3 = 17;
const uint8_t PIN_CARGA_4 = 26;

// ─────────────────── CONFIGURACIÓN PWM ──────────────────────
const uint32_t PWM_FREC = 5000;
const uint8_t  PWM_RES  = 8;

WiFiUDP udp;
const uint8_t TAM_PAQUETE = 8;
uint8_t bufferRX[TAM_PAQUETE];

bool modoCliente = false;

void configurarPWM() {
  ledcAttach(PIN_DANO_1,  PWM_FREC, PWM_RES);
  ledcAttach(PIN_DANO_2,  PWM_FREC, PWM_RES);
  ledcAttach(PIN_DANO_3,  PWM_FREC, PWM_RES);
  ledcAttach(PIN_DANO_4,  PWM_FREC, PWM_RES);
  ledcAttach(PIN_CARGA_1, PWM_FREC, PWM_RES);
  ledcAttach(PIN_CARGA_2, PWM_FREC, PWM_RES);
  ledcAttach(PIN_CARGA_3, PWM_FREC, PWM_RES);
  ledcAttach(PIN_CARGA_4, PWM_FREC, PWM_RES);

  ledcWrite(PIN_DANO_1, 0); ledcWrite(PIN_DANO_2, 0);
  ledcWrite(PIN_DANO_3, 0); ledcWrite(PIN_DANO_4, 0);
  ledcWrite(PIN_CARGA_1,0); ledcWrite(PIN_CARGA_2,0);
  ledcWrite(PIN_CARGA_3,0); ledcWrite(PIN_CARGA_4,0);
}

void setup() {
  Serial.begin(115200);
  delay(200);
  Serial.println("[ESP32 SALIDAS] Iniciando...");

  configurarPWM();

  // Intentar conectarse como cliente al AP existente
  WiFi.mode(WIFI_STA);
  WiFi.config(IPAddress().fromString(STATIC_IP),
              IPAddress().fromString(GATEWAY),
              IPAddress().fromString(SUBNET));
  WiFi.begin(AP_SSID, AP_PASSWORD);

  Serial.print("[WiFi] Conectando a " + String(AP_SSID));
  int intentos = 0;
  while (WiFi.status() != WL_CONNECTED && intentos < 20) {  // ~10 segundos
    delay(500); Serial.print(".");
    intentos++;
  }

  if (WiFi.status() == WL_CONNECTED) {
    Serial.println("\n[WiFi] Conectado como cliente. IP: " + WiFi.localIP().toString());
    modoCliente = true;
  } else {
    // No encontró la red de la placa de entradas -> crear su propio AP
    Serial.println("\n[WiFi] No se encontró AP existente. Creando AP propio...");
    WiFi.mode(WIFI_AP);
    bool apOk = WiFi.softAP(AP_SSID, AP_PASSWORD);
    if (apOk) {
      IPAddress miIP = WiFi.softAPIP();   // 192.168.4.1
      Serial.println("[AP] Red creada. IP de esta placa: " + miIP.toString());
      modoCliente = false;
    } else {
      Serial.println("[ERROR] No se pudo crear AP. Reiniciando...");
      ESP.restart();
    }
  }

  udp.begin(LOCAL_PORT);
  Serial.println("[UDP] Escuchando en puerto " + String(LOCAL_PORT));
}

void loop() {
  int len = udp.parsePacket();
  if (len >= TAM_PAQUETE) {
    udp.read(bufferRX, TAM_PAQUETE);
    ledcWrite(PIN_DANO_1,  bufferRX[0]);
    ledcWrite(PIN_DANO_2,  bufferRX[1]);
    ledcWrite(PIN_DANO_3,  bufferRX[2]);
    ledcWrite(PIN_DANO_4,  bufferRX[3]);
    ledcWrite(PIN_CARGA_1, bufferRX[4]);
    ledcWrite(PIN_CARGA_2, bufferRX[5]);
    ledcWrite(PIN_CARGA_3, bufferRX[6]);
    ledcWrite(PIN_CARGA_4, bufferRX[7]);
    udp.flush();
  }
}