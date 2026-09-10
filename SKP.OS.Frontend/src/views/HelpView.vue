<script setup lang="ts">
interface HelpSection {
  summary: string;
  body: string[];
}

const sections: HelpSection[] = [
  {
    summary: "Sådan tjekker du ind",
    body: [
      "Gå til siden 'Tjek ind' og vælg det lokale du er i.",
      "Vælg din faste plads, og du er herefter tjekket ind.",
      "Du skal være forbundet til skolens Wi-Fi for at kunne tjekke ind.",
      "Når dagen er slut, tjekker du ud i samme visning.",
    ],
  },
  {
    summary: "FF-timer",
    body: [
      "Du får automatisk 3 timer i FF-saldoen hver dag du er mødt ind.",
      "Du kan maksimalt have 37 timer i saldoen – overskuddet skaber du inden du når grænsen.",
      "Din underviser kan trække timer fra og lægge ekstra timer til. Spørg din underviser, hvis du mener din saldo er forkert.",
    ],
  },
  {
    summary: "Logbog",
    body: [
      "I logbogen skriver du kort om, hvad du har lavet i dag, og om du har søgt job.",
      "Dine logbogsindlæg gemmes per dag og kan kun ændres samme dag.",
    ],
  },
  {
    summary: "Projekter",
    body: [
      "Under 'Mine Projekter' kan du oprette dine egne projekter og koble dem til en template.",
      "Under 'SKP Projekter' kan du se skolepraktik-projekter du er tilknyttet.",
      "Du kan tilføje din Git-repo, så din underviser kan hjælpe med gennemgang og evaluering.",
    ],
  },
  {
    summary: "Kontakt og hjælp",
    body: [
      "Har du brug for hjælp eller oplysninger der skal rettes, så tag fat i din underviser.",
      "Ved tekniske problemer med systemet kan du fortælle din underviser, hvad der gik galt.",
    ],
  },
];
</script>

<template>
  <div class="page-container">
    <div class="page-header">
      <h1 class="page-title">Hjælp</h1>
      <p class="page-sub">Korte svar på de almindelige spørgsmål</p>
    </div>

    <div class="surface">
      <details v-for="section in sections" :key="section.summary" class="help-section">
        <summary class="help-summary">{{ section.summary }}</summary>
        <ul class="help-body">
          <li v-for="point in section.body" :key="point">{{ point }}</li>
        </ul>
      </details>
    </div>
  </div>
</template>

<style scoped>
.page-container {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.page-header {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.page-title {
  font-size: 24px;
  font-weight: 700;
  color: #111827;
  letter-spacing: -0.5px;
  margin: 0;
}

.page-sub {
  font-size: 15px;
  color: #6b7280;
  margin: 0;
}

.surface {
  background: #ffffff;
  border: 1px solid #e5e7eb;
  border-radius: 12px;
  padding: 6px 20px;
}

.help-section {
  border-bottom: 1px solid #f3f4f6;
}

.help-section:last-child {
  border-bottom: none;
}

.help-summary {
  cursor: pointer;
  list-style: none;
  font-size: 15px;
  font-weight: 600;
  color: #111827;
  padding: 16px 0;
  position: relative;
}

.help-summary::-webkit-details-marker {
  display: none;
}

.help-summary::after {
  content: "+";
  position: absolute;
  right: 0;
  top: 50%;
  transform: translateY(-50%);
  font-size: 18px;
  font-weight: 500;
  color: #2563eb;
}

.help-section[open] .help-summary::after {
  content: "–";
}

.help-body {
  margin: 0;
  padding: 0 0 16px;
  list-style: none;
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.help-body li {
  font-size: 14px;
  line-height: 1.5;
  color: #4b5563;
  padding-left: 16px;
  position: relative;
}

.help-body li::before {
  content: "";
  position: absolute;
  left: 0;
  top: 8px;
  width: 6px;
  height: 6px;
  border-radius: 50%;
  background: #bfdbfe;
}
</style>