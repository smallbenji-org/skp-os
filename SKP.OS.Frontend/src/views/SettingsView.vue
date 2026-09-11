<script setup lang="ts">
import { ref, computed, onMounted } from "vue";
import { useAuthStore } from "@/Stores/AuthStore";
import { useStudentProfileStore } from "@/Stores/StudentProfileStore";

const authStore = useAuthStore();
const studentProfileStore = useStudentProfileStore();

const isStudent = computed(() => authStore.HAS_ROLE("Student"));
const isLoading = ref(true);

const myProfile = computed(() => studentProfileStore.MY_STUDENT_PROFILE);

onMounted(async () => {
  try {
    if (isStudent.value) {
      await studentProfileStore.GET_MY_STUDENT_PROFILE();
    }
  } finally {
    isLoading.value = false;
  }
});
</script>

<template>
  <div class="page-container">
    <div class="page-header">
      <h1 class="page-title">Indstillinger</h1>
    </div>

    <div v-if="isLoading" class="skeleton-surface" />

    <template v-else>
      <div class="surface">
        <h2 class="surface-title">Konto</h2>
        <dl class="detail-list">
          <div class="detail-row">
            <dt>Navn</dt>
            <dd>{{ authStore.ME?.name || "—" }}</dd>
          </div>
          <div class="detail-row">
            <dt>Email</dt>
            <dd>{{ authStore.ME?.email || "—" }}</dd>
          </div>
          <div class="detail-row">
            <dt>Roller</dt>
            <dd class="role-badges">
              <span v-for="role in authStore.ROLES" :key="role" class="role-badge">{{ role }}</span>
            </dd>
          </div>
        </dl>
      </div>

      <div v-if="isStudent && myProfile" class="surface">
        <h2 class="surface-title">Elevprofil</h2>
        <dl class="detail-list">
          <div class="detail-row">
            <dt>Uddannelsesretning</dt>
            <dd>{{ myProfile.studentType }}</dd>
          </div>
          <div class="detail-row">
            <dt>Kontrakttype</dt>
            <dd>{{ myProfile.contractType }}</dd>
          </div>
          <div class="detail-row">
            <dt>EUX</dt>
            <dd>{{ myProfile.isEuxStudent ? "Ja" : "Nej" }}</dd>
          </div>
        </dl>
      </div>

      <div class="surface note-surface">
        <p class="note-text">
          Kontakt din underviser hvis dine oplysninger skal ændres, eller hvis du har brug for hjælp med at
          logge ind. Adgangskode og brugernavn kan ikke ændres her.
        </p>
      </div>
    </template>
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

.surface {
  background: #ffffff;
  border: 1px solid #e5e7eb;
  border-radius: 12px;
  padding: 20px;
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.surface-title {
  margin: 0;
  font-size: 13px;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.04em;
  color: #6b7280;
}

.detail-list {
  margin: 0;
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.detail-row {
  display: flex;
  align-items: baseline;
  justify-content: space-between;
  gap: 20px;
  padding-bottom: 12px;
  border-bottom: 1px solid #f3f4f6;
}

.detail-row:last-child {
  padding-bottom: 0;
  border-bottom: none;
}

.detail-row dt {
  font-size: 14px;
  color: #6b7280;
}

.detail-row dd {
  margin: 0;
  font-size: 14px;
  font-weight: 600;
  color: #111827;
  text-align: right;
}

.role-badges {
  display: flex;
  gap: 6px;
  flex-wrap: wrap;
  justify-content: flex-end;
}

.role-badge {
  font-size: 12px;
  font-weight: 600;
  color: #2563eb;
  background: #eef2ff;
  border-radius: 999px;
  padding: 2px 10px;
}

.note-surface {
  background: #fffbeb;
  border-color: #fde68a;
}

.note-text {
  margin: 0;
  font-size: 14px;
  line-height: 1.5;
  color: #92400e;
}
</style>