<script setup lang="ts">
import { ref, computed, onMounted, watch, watchEffect } from "vue";
import {
  IconInfoCircle,
  IconChevronDown,
  IconChevronLeft,
  IconChevronRight,
  IconSearch,
  IconLoader2,
  IconCheck,
  IconAlertCircle,
} from "@tabler/icons-vue";
import { useLogbookEntryStore } from "@/Stores/LogbookEntryStore";
import { useStudentProfileStore } from "@/Stores/StudentProfileStore";
import type { LogbookEntryDto } from "@/types";

const logbookStore = useLogbookEntryStore();
const studentProfileStore = useStudentProfileStore();

const isLoading = ref(true);
const isSaving = ref(false);
const saveStatus = ref<"idle" | "success" | "error">("idle");

const INFO_EXPANDED_KEY = "logbook_info_expanded";
const infoExpanded = ref(localStorage.getItem(INFO_EXPANDED_KEY) !== "false");
watchEffect(() => {
  localStorage.setItem(INFO_EXPANDED_KEY, String(infoExpanded.value));
});

const todayEntry = ref("");
const hasApplied = ref(false);
const MIN_CHARS = 50;
const MAX_CHARS = 1400;

const searchQuery = ref("");
const rowsPerPage = ref(10);
const currentPage = ref(1);

const today = new Date();
const todayISO = today.toISOString().split("T")[0];

const todayLabel = computed(() =>
  today.toLocaleDateString("da-DK", {
    weekday: "long",
    day: "numeric",
    month: "long",
    year: "numeric",
  }),
);

const todayExistingEntry = computed<LogbookEntryDto | null>(
  () =>
    logbookStore.LOGBOOK_ENTRIES.find((e) => e.date.startsWith(todayISO)) ??
    null,
);

const charCount = computed(() => todayEntry.value.length);
const charsLeft = computed(() => MAX_CHARS - charCount.value);
const charWarning = computed(() => charsLeft.value <= 200);
const isTooShort = computed(() => todayEntry.value.trim().length < MIN_CHARS);
const charsRemaining = computed(
  () => MIN_CHARS - todayEntry.value.trim().length,
);

const filteredEntries = computed(() => {
  const q = searchQuery.value.trim().toLowerCase();
  const sorted = [...logbookStore.LOGBOOK_ENTRIES].sort(
    (a, b) => new Date(b.date).getTime() - new Date(a.date).getTime(),
  );
  if (!q) return sorted;
  return sorted.filter(
    (e) =>
      formatDate(e.date).toLowerCase().includes(q) ||
      e.entry.toLowerCase().includes(q),
  );
});

const totalRows = computed(() => filteredEntries.value.length);
const totalPages = computed(() =>
  Math.max(1, Math.ceil(totalRows.value / rowsPerPage.value)),
);

const pagedEntries = computed(() => {
  const start = (currentPage.value - 1) * rowsPerPage.value;
  return filteredEntries.value.slice(start, start + rowsPerPage.value);
});

const pageStart = computed(() =>
  totalRows.value === 0 ? 0 : (currentPage.value - 1) * rowsPerPage.value + 1,
);
const pageEnd = computed(() =>
  Math.min(currentPage.value * rowsPerPage.value, totalRows.value),
);

const visiblePages = computed(() => {
  const total = totalPages.value;
  const cur = currentPage.value;
  if (total <= 7) return Array.from({ length: total }, (_, i) => i + 1);
  const pages: (number | "...")[] = [1];
  if (cur > 3) pages.push("...");
  for (let p = Math.max(2, cur - 1); p <= Math.min(total - 1, cur + 1); p++)
    pages.push(p);
  if (cur < total - 2) pages.push("...");
  pages.push(total);
  return pages;
});

watch(searchQuery, () => {
  currentPage.value = 1;
});
watch(rowsPerPage, () => {
  currentPage.value = 1;
});

function formatDate(dateStr: string) {
  return new Date(dateStr).toLocaleDateString("da-DK", {
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
  });
}

function goToPage(p: number | "...") {
  if (typeof p === "number") currentPage.value = p;
}

function prevPage() {
  if (currentPage.value > 1) currentPage.value--;
}
function nextPage() {
  if (currentPage.value < totalPages.value) currentPage.value++;
}

async function save() {
  if (isTooShort.value || isSaving.value) return;
  isSaving.value = true;
  saveStatus.value = "idle";

  const profile = studentProfileStore.MY_STUDENT_PROFILE;
  if (!profile) {
    saveStatus.value = "error";
    isSaving.value = false;
    return;
  }

  const payload = {
    date: todayISO,
    entry: todayEntry.value.trim(),
    hasSearchedForJob: hasApplied.value,
    studentProfileId: profile.id,
  };

  const result = todayExistingEntry.value
    ? await logbookStore.UPDATE_LOGBOOK_ENTRY(
        todayExistingEntry.value.id,
        payload,
      )
    : await logbookStore.CREATE_LOGBOOK_ENTRY(payload);

  saveStatus.value = result ? "success" : "error";
  isSaving.value = false;
  if (result)
    setTimeout(() => {
      saveStatus.value = "idle";
    }, 3000);
}

onMounted(async () => {
  await Promise.all([
    studentProfileStore.GET_MY_STUDENT_PROFILE(),
    logbookStore.GET_LOGBOOK_ENTRIES(),
  ]);
  if (todayExistingEntry.value) {
    todayEntry.value = todayExistingEntry.value.entry;
    hasApplied.value = todayExistingEntry.value.hasSearchedForJob;
  }
  isLoading.value = false;
});
</script>

<template>
  <div class="page-container">
    <div class="page-header">
      <h1 class="page-title">Logbog</h1>
    </div>

    <template v-if="isLoading">
      <div class="skeleton-surface" />
    </template>

    <template v-else>
      <div class="surface">
        <section class="section section-info">
          <button
            class="info-toggle"
            :aria-expanded="infoExpanded"
            @click="infoExpanded = !infoExpanded"
          >
            <span class="info-toggle-left">
              <IconInfoCircle :size="13" :stroke-width="2.2" />
              <span>Vejledning</span>
            </span>
            <IconChevronDown
              :size="14"
              :stroke-width="2.2"
              class="info-chevron"
              :class="{ expanded: infoExpanded }"
            />
          </button>

          <Transition name="info-collapse">
            <div v-if="infoExpanded" class="info-content">
              <p class="info-body">
                Logbogen er din daglige dokumentation af arbejdet med dit
                projekt. Her skriver du kort, hvad du har arbejdet med, hvordan
                det går, eventuelle problemer og hvad du skal arbejde videre
                med.
              </p>
              <p class="info-req-label">
                Hvert indlæg skal <strong>ALTID</strong> besvare:
              </p>
              <ul class="info-list">
                <li>Hvad har jeg lavet siden sidst?</li>
                <li>Hvordan går det i forhold til planen?</li>
                <li>Er jeg stødt ind i problemer (hvilke)?</li>
                <li>Plan for næste dag – hvad skal jeg arbejde med?</li>
              </ul>
              <p class="info-note">
                <strong>BEMÆRK:</strong> Marker
                <em>"Jeg har indsendt en ansøgning i dag"</em>
                <strong>KUN</strong> hvis du har sendt en ansøgning den dag.
              </p>
            </div>
          </Transition>
        </section>

        <div v-show="infoExpanded" class="divider" />

        <section class="section section-today">
          <div class="today-heading">
            <span class="today-date">{{ todayLabel }}</span>
            <span
              class="status-meta"
              :class="todayExistingEntry ? 'written' : 'missing'"
            >
              <span class="dot" />
              {{ todayExistingEntry ? "Skrevet i dag" : "Ikke skrevet endnu" }}
            </span>
          </div>

          <textarea
            id="log-entry-textarea"
            v-model="todayEntry"
            class="log-textarea"
            placeholder="Skriv dagens indlæg…"
            :maxlength="MAX_CHARS"
            :minlength="50"
            rows="5"
          />

          <div class="entry-footer">
            <label class="checkbox-label" for="log-applied-checkbox">
              <span
                class="custom-checkbox"
                :class="{ checked: hasApplied }"
                role="checkbox"
                :aria-checked="hasApplied"
                tabindex="0"
                @click="hasApplied = !hasApplied"
                @keydown.space.prevent="hasApplied = !hasApplied"
                @keydown.enter.prevent="hasApplied = !hasApplied"
              >
                <IconCheck v-if="hasApplied" :size="11" :stroke-width="3" />
              </span>
              <input
                id="log-applied-checkbox"
                type="checkbox"
                v-model="hasApplied"
                class="sr-only"
              />
              <span class="checkbox-text"
                >Jeg har sendt en ansøgning i dag.</span
              >
            </label>

            <div class="entry-footer-right">
              <span class="char-count" :class="{ warning: charWarning }">
                {{ charCount.toLocaleString("da-DK") }} /
                {{ MAX_CHARS.toLocaleString("da-DK") }} tegn
              </span>

              <Transition name="status-fade">
                <span
                  v-if="saveStatus === 'success'"
                  class="save-feedback success"
                >
                  <IconCheck :size="13" :stroke-width="2.5" /> Gemt
                </span>
                <span
                  v-else-if="saveStatus === 'error'"
                  class="save-feedback error"
                >
                  <IconAlertCircle :size="13" :stroke-width="2" /> Fejl
                </span>
              </Transition>

              <span
                v-if="isTooShort && todayEntry.trim().length > 0"
                class="min-chars-hint"
              >
                {{ charsRemaining }} tegn mangler
              </span>

              <button
                class="save-btn"
                :disabled="isTooShort || isSaving"
                @click="save"
              >
                <IconLoader2
                  v-if="isSaving"
                  :size="14"
                  :stroke-width="2"
                  class="spin-icon"
                />
                Gem indlæg
              </button>
            </div>
          </div>
        </section>

        <div class="divider" />

        <section class="section section-table">
          <div class="section-heading">Tidligere indlæg</div>
          <div class="table-toolbar">
            <div class="rows-select-group">
              <label for="rows-per-page" class="toolbar-label">Vis</label>
              <select
                id="rows-per-page"
                v-model="rowsPerPage"
                class="rows-select"
              >
                <option :value="10">10</option>
                <option :value="25">25</option>
                <option :value="50">50</option>
              </select>
              <span class="toolbar-label">rækker pr. side</span>
            </div>

            <div class="search-wrapper">
              <IconSearch :size="14" :stroke-width="2" class="search-icon" />
              <input
                id="log-search"
                v-model="searchQuery"
                type="text"
                class="search-input"
                placeholder="Søg…"
                aria-label="Søg i logbog"
              />
            </div>
          </div>

          <table class="log-table" aria-label="Logbogsindlæg">
            <thead>
              <tr>
                <th class="col-date">Dato</th>
                <th class="col-entry">Indhold</th>
              </tr>
            </thead>
            <tbody>
              <tr v-if="pagedEntries.length === 0">
                <td colspan="2" class="empty-row">Ingen indlæg fundet.</td>
              </tr>
              <tr
                v-for="entry in pagedEntries"
                :key="entry.id"
                class="table-row"
                :class="{ 'row-today': entry.date.startsWith(todayISO) }"
              >
                <td class="cell-date">
                  {{ formatDate(entry.date) }}
                  <span
                    v-if="entry.hasSearchedForJob"
                    class="applied-chip"
                    title="Sendte ansøgning denne dag"
                    >✉</span
                  >
                </td>
                <td class="cell-entry">{{ entry.entry }}</td>
              </tr>
            </tbody>
          </table>

          <div class="table-footer">
            <span class="rows-count">
              Viser {{ pageStart }} til {{ pageEnd }} af {{ totalRows }} rækker
            </span>

            <nav class="pagination" aria-label="Pagination">
              <button
                class="page-btn"
                :disabled="currentPage === 1"
                @click="prevPage"
                aria-label="Forrige side"
              >
                <IconChevronLeft :size="14" :stroke-width="2.2" />
              </button>
              <template v-for="(p, i) in visiblePages" :key="i">
                <span v-if="p === '...'" class="page-ellipsis">…</span>
                <button
                  v-else
                  class="page-btn"
                  :class="{ active: p === currentPage }"
                  @click="goToPage(p)"
                  :aria-current="p === currentPage ? 'page' : undefined"
                >
                  {{ p }}
                </button>
              </template>
              <button
                class="page-btn"
                :disabled="currentPage === totalPages"
                @click="nextPage"
                aria-label="Næste side"
              >
                <IconChevronRight :size="14" :stroke-width="2.2" />
              </button>
            </nav>
          </div>
        </section>
      </div>
    </template>
  </div>
</template>

<style scoped>
.page-container {
  display: flex;
  flex-direction: column;
  gap: 16px;
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
}

.surface {
  background: #ffffff;
  border: 1px solid #dde1e5;
  border-radius: 10px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.06);
  overflow: hidden;
}

.skeleton-surface {
  height: 640px;
  background: linear-gradient(90deg, #e5eaed 25%, #eff2f4 50%, #e5eaed 75%);
  background-size: 400% 100%;
  animation: shimmer 1.4s ease infinite;
  border-radius: 10px;
}

@keyframes shimmer {
  0% {
    background-position: 100% 0;
  }
  100% {
    background-position: -100% 0;
  }
}

.section {
  padding: 18px 22px;
}

.section-table {
  padding-top: 22px;
}

.section-heading {
  font-size: 12px;
  font-weight: 700;
  color: #6b7280;
  margin-bottom: 14px;
}

.divider {
  height: 1px;
  background: #dde1e5;
}

.section-info {
  background: #f8fafc;
}

.info-toggle {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  background: none;
  border: none;
  padding: 0;
  cursor: pointer;
  font-family: inherit;
  outline: none;
}

.info-toggle:focus-visible {
  outline: 2px solid #016bff;
  outline-offset: 2px;
  border-radius: 4px;
}

.info-toggle-left {
  display: flex;
  align-items: center;
  gap: 5px;
  font-size: 11px;
  font-weight: 700;
  color: #6b7280;
}

.info-chevron {
  color: #9ca3af;
  transition: transform 0.25s cubic-bezier(0.4, 0, 0.2, 1);
}

.info-chevron.expanded {
  transform: rotate(180deg);
}

.info-content {
  overflow: hidden;
}

.info-body {
  font-size: 13px;
  color: #4b5563;
  line-height: 1.6;
  margin-top: 10px;
  margin-bottom: 10px;
  max-width: 72ch;
}

.info-req-label {
  font-size: 12.5px;
  font-weight: 600;
  color: #111827;
  margin-bottom: 6px;
}

.info-list {
  margin: 0 0 10px 18px;
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.info-list li {
  font-size: 12.5px;
  color: #4b5563;
  line-height: 1.5;
}

.info-note {
  font-size: 12px;
  color: #6b7280;
  line-height: 1.5;
}

.today-heading {
  display: flex;
  flex-direction: column;
  gap: 3px;
  margin-bottom: 10px;
}

.today-date {
  font-size: 14px;
  font-weight: 700;
  color: #111827;
  text-transform: capitalize;
}

.status-meta {
  display: inline-flex;
  align-items: center;
  gap: 5px;
  font-size: 11.5px;
  font-weight: 500;
  color: #6b7280;
}

.dot {
  width: 6px;
  height: 6px;
  border-radius: 50%;
  flex-shrink: 0;
}

.status-meta.missing .dot {
  background: #d4a017;
}

.status-meta.written .dot {
  background: #4ade80;
}

.log-textarea {
  width: 100%;
  resize: vertical;
  background: #f8fafc;
  border: 1px solid #e2e8f0;
  border-radius: 6px;
  padding: 10px 12px;
  font-family: inherit;
  font-size: 13px;
  color: #111827;
  line-height: 1.6;
  outline: none;
  transition:
    border-color 0.2s,
    background-color 0.2s;
}

.log-textarea::placeholder {
  color: #9ca3af;
}

.log-textarea:focus {
  border-color: #016bff;
  background: #ffffff;
}

.entry-footer {
  display: flex;
  align-items: center;
  justify-content: space-between;
  flex-wrap: wrap;
  gap: 10px;
  margin-top: 10px;
}

.entry-footer-right {
  display: flex;
  align-items: center;
  gap: 12px;
}

.char-count {
  font-size: 12px;
  font-weight: 500;
  color: #9ca3af;
  transition: color 0.2s;
}

.char-count.warning {
  color: #b45309;
  font-weight: 600;
}

.checkbox-label {
  display: flex;
  align-items: center;
  gap: 8px;
  cursor: pointer;
  user-select: none;
}

.sr-only {
  position: absolute;
  width: 1px;
  height: 1px;
  padding: 0;
  margin: -1px;
  overflow: hidden;
  clip: rect(0, 0, 0, 0);
  white-space: nowrap;
  border: 0;
}

.custom-checkbox {
  width: 17px;
  height: 17px;
  border-radius: 4px;
  border: 1.5px solid #d1d5db;
  background: #f8fafc;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  transition:
    border-color 0.15s,
    background 0.15s;
  cursor: pointer;
}

.custom-checkbox:focus-visible {
  outline: 2px solid #016bff;
  outline-offset: 2px;
}

.custom-checkbox.checked {
  border-color: #016bff;
  background: #016bff;
  color: #fff;
}

.checkbox-text {
  font-size: 12.5px;
  font-weight: 500;
  color: #4b5563;
}

.save-feedback {
  display: flex;
  align-items: center;
  gap: 4px;
  font-size: 12px;
  font-weight: 600;
}

.save-feedback.success {
  color: #166534;
}
.save-feedback.error {
  color: #991b1b;
}

.min-chars-hint {
  font-size: 12px;
  font-weight: 500;
  color: #b45309;
}

.save-btn {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 6px;
  height: 36px;
  padding: 0 18px;
  background: #016bff;
  color: #ffffff;
  border: none;
  border-radius: 6px;
  font-size: 13px;
  font-weight: 700;
  cursor: pointer;
  font-family: inherit;
  outline: none;
  transition:
    background-color 0.2s,
    transform 0.1s,
    opacity 0.2s;
}

.save-btn:hover:not(:disabled) {
  background: #005ae0;
}
.save-btn:active:not(:disabled) {
  transform: scale(0.97);
}
.save-btn:disabled {
  opacity: 0.4;
  cursor: not-allowed;
}

.spin-icon {
  animation: spin 0.8s linear infinite;
}

@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}

.table-toolbar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  flex-wrap: wrap;
  gap: 10px;
  margin-bottom: 12px;
}

.rows-select-group {
  display: flex;
  align-items: center;
  gap: 7px;
}

.toolbar-label {
  font-size: 12.5px;
  font-weight: 500;
  color: #6b7280;
}

.rows-select {
  height: 30px;
  padding: 0 8px;
  background: #f8fafc;
  border: 1px solid #e2e8f0;
  border-radius: 6px;
  font-family: inherit;
  font-size: 12.5px;
  font-weight: 600;
  color: #111827;
  outline: none;
  cursor: pointer;
  transition: border-color 0.2s;
}

.rows-select:focus {
  border-color: #016bff;
}

.search-wrapper {
  position: relative;
  display: flex;
  align-items: center;
}

.search-icon {
  position: absolute;
  left: 9px;
  color: #9ca3af;
  pointer-events: none;
  transition: color 0.2s;
}

.search-input {
  height: 30px;
  width: 180px;
  padding: 0 10px 0 28px;
  background: #f8fafc;
  border: 1px solid #e2e8f0;
  border-radius: 6px;
  font-family: inherit;
  font-size: 12.5px;
  color: #111827;
  outline: none;
  transition:
    border-color 0.2s,
    background-color 0.2s;
}

.search-input::placeholder {
  color: #9ca3af;
}
.search-input:focus {
  border-color: #016bff;
  background: #ffffff;
}
.search-wrapper:focus-within .search-icon {
  color: #016bff;
}

.log-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 13px;
}

.log-table thead tr {
  border-bottom: 1px solid #e5e7eb;
}

.log-table th {
  padding: 7px 12px;
  font-size: 11px;
  font-weight: 700;
  color: #9ca3af;
  text-align: left;
}

.col-date {
  width: 150px;
}

.table-row {
  border-bottom: 1px solid #f3f4f6;
  transition: background-color 0.12s;
}

.table-row:last-child {
  border-bottom: none;
}
.table-row:hover {
  background: #f9fafb;
}
.table-row.row-today {
  background: #eff6ff;
}
.table-row.row-today:hover {
  background: #dbeafe;
}

.log-table td {
  padding: 9px 12px;
  vertical-align: middle;
}

.cell-date {
  font-size: 12.5px;
  font-weight: 600;
  color: #374151;
  white-space: nowrap;
}

.applied-chip {
  display: inline-block;
  font-size: 10.5px;
  color: #016bff;
  background: #eff6ff;
  border: 1px solid #bfdbfe;
  border-radius: 3px;
  padding: 0 4px;
  margin-left: 5px;
  cursor: default;
}

.cell-entry {
  font-size: 13px;
  color: #4b5563;
  line-height: 1.45;
  max-width: 0;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.empty-row {
  text-align: center;
  color: #9ca3af;
  font-size: 12.5px;
  padding: 28px 0;
}

.table-footer {
  display: flex;
  align-items: center;
  justify-content: space-between;
  flex-wrap: wrap;
  gap: 10px;
  margin-top: 12px;
}

.rows-count {
  font-size: 12px;
  color: #9ca3af;
  font-weight: 500;
}

.pagination {
  display: flex;
  align-items: center;
  gap: 3px;
}

.page-btn {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  min-width: 30px;
  height: 30px;
  padding: 0 5px;
  border: 1px solid #e2e8f0;
  border-radius: 6px;
  background: #ffffff;
  color: #374151;
  font-size: 12.5px;
  font-weight: 600;
  font-family: inherit;
  cursor: pointer;
  outline: none;
  transition:
    border-color 0.15s,
    background-color 0.15s,
    color 0.15s,
    transform 0.1s;
}

.page-btn:hover:not(:disabled):not(.active) {
  border-color: #016bff;
  color: #016bff;
}

.page-btn:active:not(:disabled):not(.active) {
  transform: scale(0.95);
}

.page-btn.active {
  background: #016bff;
  border-color: #016bff;
  color: #ffffff;
  cursor: default;
}

.page-btn:disabled {
  opacity: 0.35;
  cursor: not-allowed;
}

.page-ellipsis {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 26px;
  font-size: 12.5px;
  color: #9ca3af;
  user-select: none;
}

.info-collapse-enter-active,
.info-collapse-leave-active {
  transition:
    max-height 0.28s cubic-bezier(0.4, 0, 0.2, 1),
    opacity 0.22s ease;
  overflow: hidden;
  max-height: 400px;
}

.info-collapse-enter-from,
.info-collapse-leave-to {
  max-height: 0;
  opacity: 0;
}

.status-fade-enter-active,
.status-fade-leave-active {
  transition:
    opacity 0.18s ease,
    transform 0.18s ease;
}

.status-fade-enter-from,
.status-fade-leave-to {
  opacity: 0;
  transform: translateY(3px);
}
</style>
