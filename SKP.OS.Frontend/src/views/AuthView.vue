<script setup lang="ts">
import { ref, reactive, watch, nextTick, onMounted, onUnmounted } from 'vue'
import {
  IconUser,
  IconMail,
  IconLock,
  IconEye,
  IconEyeOff,
  IconAlertCircle,
  IconCheck,
  IconLoader2,
  IconArrowRight
} from '@tabler/icons-vue'
import { useAuthStore } from '@/Stores/AuthStore'

const props = withDefaults(
  defineProps<{
    initialMode?: 'login' | 'register'
  }>(),
  {
    initialMode: 'login',
  }
)

const emit = defineEmits<{
  (e: 'success'): void
}>()

const authStore = useAuthStore()
const loading = ref(false)

const currentMode = ref<'login' | 'register'>(props.initialMode)
const showPassword = ref(false)
const errorMessage = ref('')
const successMessage = ref('')

const authContainerRef = ref<HTMLElement | null>(null)
const loginPaneRef = ref<HTMLElement | null>(null)
const registerPaneRef = ref<HTMLElement | null>(null)
const containerHeight = ref<number | undefined>(undefined)
const isTransitioning = ref(false)

const loginForm = reactive({
  email: '',
  password: '',
})

const registerForm = reactive({
  name: '',
  email: '',
  password: '',
})

const measureTargetHeight = (mode: 'login' | 'register') => {
  if (!authContainerRef.value) return undefined
  const activePane = mode === 'login' ? loginPaneRef.value : registerPaneRef.value
  const currentPane = currentMode.value === 'login' ? loginPaneRef.value : registerPaneRef.value
  if (!activePane || !currentPane) return undefined
  const diff = activePane.offsetHeight - currentPane.offsetHeight
  return authContainerRef.value.offsetHeight + diff
}

const updateHeight = () => {
  if (authContainerRef.value) {
    containerHeight.value = authContainerRef.value.offsetHeight
  }
}

const switchMode = (mode: 'login' | 'register') => {
  if (currentMode.value === mode || isTransitioning.value) return
  errorMessage.value = ''
  successMessage.value = ''
  showPassword.value = false

  if (mode === 'register') {
    const targetHeight = measureTargetHeight('register')
    if (targetHeight) {
      isTransitioning.value = true
      containerHeight.value = targetHeight
      setTimeout(() => {
        currentMode.value = 'register'
        isTransitioning.value = false
      }, 350)
      return
    }
  }

  if (mode === 'login') {
    const targetHeight = measureTargetHeight('login')
    currentMode.value = 'login'
    if (targetHeight) {
      containerHeight.value = targetHeight
    }
    return
  }

  currentMode.value = mode
}

watch(currentMode, () => {
  nextTick(() => {
    updateHeight()
  })
})

onMounted(() => {
  nextTick(() => {
    updateHeight()
  })
  window.addEventListener('resize', updateHeight)
})

onUnmounted(() => {
  window.removeEventListener('resize', updateHeight)
})

const handleLogin = async () => {
  errorMessage.value = ''
  successMessage.value = ''

  if (!loginForm.email.trim()) {
    errorMessage.value = 'Indtast venligst din e-mailadresse.'
    return
  }
  if (!loginForm.password) {
    errorMessage.value = 'Indtast venligst din adgangskode.'
    return
  }

  loading.value = true
  try {
    const user = await authStore.LOGIN({
      userName: loginForm.email.trim(),
      password: loginForm.password,
    })

    if (user) {
      emit('success')
    } else {
      errorMessage.value = 'Kunne ikke logge ind. Tjek dine oplysninger.'
    }
  } catch {
    errorMessage.value = 'Der opstod en fejl under login. Prøv venligst igen.'
  } finally {
    loading.value = false
  }
}

const handleRegister = async () => {
  errorMessage.value = ''
  successMessage.value = ''

  if (!registerForm.name.trim()) {
    errorMessage.value = 'Indtast venligst dit fulde navn.'
    return
  }
  if (!registerForm.email.trim()) {
    errorMessage.value = 'Indtast venligst din e-mailadresse.'
    return
  }
  if (!registerForm.password || registerForm.password.length < 8) {
    errorMessage.value = 'Adgangskoden skal være mindst 8 tegn lang og indeholde store/små bogstaver, tal og specialtegn.'
    return
  }

  const emailTrimmed = registerForm.email.trim()

  loading.value = true
  try {
    const user = await authStore.REGISTER({
      name: registerForm.name.trim(),
      userName: emailTrimmed,
      email: emailTrimmed,
      password: registerForm.password,
      role: 'Student',
    })

    if (user) {
      successMessage.value = 'Bruger oprettet med succes! Du kan nu logge ind.'
      loginForm.email = emailTrimmed
      loginForm.password = registerForm.password
      setTimeout(() => {
        switchMode('login')
      }, 1200)
    } else {
      errorMessage.value = 'Kunne ikke oprette brugeren. Tjek dine oplysninger.'
    }
  } catch {
    errorMessage.value = 'Der opstod en fejl under registrering. Prøv venligst igen.'
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="auth-wrapper">
    <div 
      ref="authContainerRef"
      class="auth-container"
      :style="{ height: containerHeight !== undefined ? `${containerHeight}px` : 'auto' }"
    >
      <div class="auth-header">
        <h1 class="auth-title">SKP OS</h1>
      </div>

      <div class="mode-toggle">
        <div 
          class="mode-indicator-track" 
          :style="{ transform: currentMode === 'login' ? 'translateX(0%)' : 'translateX(100%)' }"
        >
          <div class="mode-indicator" />
        </div>
        <button
          type="button"
          class="mode-btn"
          :class="{ active: currentMode === 'login' }"
          @click="switchMode('login')"
        >
          Log ind
        </button>
        <button
          type="button"
          class="mode-btn"
          :class="{ active: currentMode === 'register' }"
          @click="switchMode('register')"
        >
          Opret bruger
        </button>
      </div>

      <Transition name="fade">
        <div v-if="errorMessage" class="alert-box alert-error" role="alert">
          <IconAlertCircle :size="18" class="alert-icon" />
          <span>{{ errorMessage }}</span>
        </div>
      </Transition>

      <Transition name="fade">
        <div v-if="successMessage" class="alert-box alert-success" role="alert">
          <IconCheck :size="18" class="alert-icon" />
          <span>{{ successMessage }}</span>
        </div>
      </Transition>

      <div class="forms-viewport">
        <div 
          class="forms-track" 
          :class="{ 'slide-register': currentMode === 'register' }"
        >
          <div 
            ref="loginPaneRef" 
            class="form-pane" 
            :class="{ active: currentMode === 'login' }"
          >
            <form class="auth-form" @submit.prevent="handleLogin">
              <div class="form-group">
                <label for="login-email" class="form-label">E-mail</label>
                <div class="input-wrapper">
                  <IconMail :size="18" class="input-icon" />
                  <input
                    id="login-email"
                    v-model="loginForm.email"
                    type="email"
                    class="form-input"
                    placeholder="F.eks. navn@skp.dk"
                    autocomplete="email"
                    required
                    :disabled="loading"
                    :tabindex="currentMode === 'login' ? 0 : -1"
                  />
                </div>
              </div>

              <div class="form-group">
                <div class="label-row">
                  <label for="login-password" class="form-label">Adgangskode</label>
                </div>
                <div class="input-wrapper">
                  <IconLock :size="18" class="input-icon" />
                  <input
                    id="login-password"
                    v-model="loginForm.password"
                    :type="showPassword ? 'text' : 'password'"
                    class="form-input has-toggle"
                    placeholder="Indtast din adgangskode"
                    autocomplete="current-password"
                    required
                    :disabled="loading"
                    :tabindex="currentMode === 'login' ? 0 : -1"
                  />
                  <button
                    type="button"
                    class="password-toggle-btn"
                    :aria-label="showPassword ? 'Skjul adgangskode' : 'Vis adgangskode'"
                    :tabindex="currentMode === 'login' ? 0 : -1"
                    @click="showPassword = !showPassword"
                  >
                    <IconEyeOff v-if="showPassword" :size="18" />
                    <IconEye v-else :size="18" />
                  </button>
                </div>
              </div>

              <button
                type="submit"
                class="submit-btn"
                :disabled="loading"
                :tabindex="currentMode === 'login' ? 0 : -1"
              >
                <IconLoader2 v-if="loading" :size="20" class="spin-icon" />
                <span v-else>Log ind</span>
                <IconArrowRight v-if="!loading" :size="18" />
              </button>
            </form>
          </div>

          <div 
            ref="registerPaneRef" 
            class="form-pane" 
            :class="{ active: currentMode === 'register' }"
          >
            <form class="auth-form" @submit.prevent="handleRegister">
              <div class="form-group">
                <label for="reg-name" class="form-label">Fulde navn</label>
                <div class="input-wrapper">
                  <IconUser :size="18" class="input-icon" />
                  <input
                    id="reg-name"
                    v-model="registerForm.name"
                    type="text"
                    class="form-input"
                    placeholder="F.eks. Mikkel Martin Larsen"
                    autocomplete="name"
                    required
                    :disabled="loading"
                    :tabindex="currentMode === 'register' ? 0 : -1"
                  />
                </div>
              </div>

              <div class="form-group">
                <label for="reg-email" class="form-label">E-mail</label>
                <div class="input-wrapper">
                  <IconMail :size="18" class="input-icon" />
                  <input
                    id="reg-email"
                    v-model="registerForm.email"
                    type="email"
                    class="form-input"
                    placeholder="navn@skp.dk"
                    autocomplete="email"
                    required
                    :disabled="loading"
                    :tabindex="currentMode === 'register' ? 0 : -1"
                  />
                </div>
              </div>

              <div class="form-group">
                <label for="reg-password" class="form-label">Adgangskode</label>
                <div class="input-wrapper">
                  <IconLock :size="18" class="input-icon" />
                  <input
                    id="reg-password"
                    v-model="registerForm.password"
                    :type="showPassword ? 'text' : 'password'"
                    class="form-input has-toggle"
                    placeholder="Min. 8 tegn (A-z, 0-9, symbol)"
                    autocomplete="new-password"
                    required
                    :disabled="loading"
                    :tabindex="currentMode === 'register' ? 0 : -1"
                  />
                  <button
                    type="button"
                    class="password-toggle-btn"
                    :aria-label="showPassword ? 'Skjul adgangskode' : 'Vis adgangskode'"
                    :tabindex="currentMode === 'register' ? 0 : -1"
                    @click="showPassword = !showPassword"
                  >
                    <IconEyeOff v-if="showPassword" :size="18" />
                    <IconEye v-else :size="18" />
                  </button>
                </div>
              </div>

              <button
                type="submit"
                class="submit-btn"
                :disabled="loading"
                :tabindex="currentMode === 'register' ? 0 : -1"
              >
                <IconLoader2 v-if="loading" :size="20" class="spin-icon" />
                <span v-else>Opret bruger</span>
                <IconArrowRight v-if="!loading" :size="18" />
              </button>
            </form>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.auth-wrapper {
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 100vh;
  width: 100vw;
  background-color: #E1E6EA;
  padding: 24px 16px;
  box-sizing: border-box;
  overflow-y: auto;
}

.auth-container {
  width: 100%;
  max-width: 420px;
  background: 
    linear-gradient(white 0% 100%) padding-box,
    linear-gradient(135deg, #ffffff 60%, #cbd5e1 100%) border-box;
  border: 2px solid transparent;
  border-radius: 26px;
  box-shadow: 
    0 20px 40px -15px rgba(0, 0, 0, 0.1),
    0 4px 16px rgba(0, 0, 0, 0.04);
  padding: 28px 28px 24px;
  box-sizing: border-box;
  transition: height 0.35s cubic-bezier(0.4, 0, 0.2, 1);
  will-change: height;
  overflow: hidden;
}

.auth-header {
  display: flex;
  flex-direction: column;
  align-items: center;
  text-align: center;
  margin-bottom: 16px;
}

.auth-title {
  font-size: 24px;
  font-weight: 800;
  color: #1a1a1a;
  letter-spacing: -0.5px;
  margin: 0;
}

.mode-toggle {
  position: relative;
  display: flex;
  background-color: #f1f5f9;
  border-radius: 12px;
  padding: 3px;
  margin-bottom: 16px;
  border: 1px solid #e2e8f0;
  overflow: hidden;
}

.mode-indicator-track {
  position: absolute;
  top: 0;
  left: 0;
  width: 50%;
  height: 100%;
  padding: 3px;
  box-sizing: border-box;
  pointer-events: none;
  z-index: 1;
  transition: transform 0.35s cubic-bezier(0.4, 0, 0.2, 1);
}

.mode-indicator {
  width: 100%;
  height: 100%;
  background-color: #016BFF;
  border-radius: 9px;
}

.mode-btn {
  position: relative;
  z-index: 2;
  flex: 1;
  height: 36px;
  border: none;
  background-color: transparent;
  color: #64748b;
  border-radius: 9px;
  font-size: 13.5px;
  font-weight: 600;
  cursor: pointer;
  outline: none;
  font-family: inherit;
  transition: color 0.25s ease;
}

.mode-btn.active {
  color: #ffffff;
}

.mode-btn:hover:not(.active) {
  color: #111827;
}

.alert-box {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 8px 12px;
  border-radius: 10px;
  font-size: 12.5px;
  font-weight: 500;
  margin-bottom: 14px;
  line-height: 1.4;
}

.alert-error {
  background-color: #fef2f2;
  border: 1px solid #fecaca;
  color: #b91c1c;
}

.alert-success {
  background-color: #f0fdf4;
  border: 1px solid #bbf7d0;
  color: #15803d;
}

.alert-icon {
  flex-shrink: 0;
}

.forms-viewport {
  position: relative;
  width: 100%;
  overflow: hidden;
}

.forms-track {
  display: flex;
  width: 200%;
  transform: translateX(0%);
  transition: transform 0.38s cubic-bezier(0.4, 0, 0.2, 1);
}

.forms-track.slide-register {
  transform: translateX(-50%);
}

.form-pane {
  width: 50%;
  flex-shrink: 0;
  box-sizing: border-box;
  padding: 2px 2px 4px 2px;
  opacity: 0.2;
  transform: scale(0.97);
  pointer-events: none;
  transition: 
    opacity 0.3s cubic-bezier(0.4, 0, 0.2, 1),
    transform 0.38s cubic-bezier(0.4, 0, 0.2, 1);
}

.form-pane.active {
  opacity: 1;
  transform: scale(1);
  pointer-events: auto;
}

.auth-form {
  display: flex;
  flex-direction: column;
  height: 100%;
  gap: 12px;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 5px;
}

.label-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.form-label {
  font-size: 12.5px;
  font-weight: 600;
  color: #374151;
}

.input-wrapper {
  position: relative;
  display: flex;
  align-items: center;
}

.input-icon {
  position: absolute;
  left: 12px;
  color: #9ca3af;
  pointer-events: none;
  transition: color 0.2s ease;
}

.form-input {
  width: 100%;
  height: 40px;
  background-color: #f8fafc;
  border: 1.5px solid #e2e8f0;
  border-radius: 11px;
  padding: 0 12px 0 38px;
  font-family: inherit;
  font-size: 13px;
  color: #111827;
  outline: none;
  transition: 
    border-color 0.2s ease, 
    background-color 0.2s ease;
}

.form-input.has-toggle {
  padding-right: 38px;
}

.form-input::placeholder {
  color: #9ca3af;
}

.form-input:focus {
  border-color: #016BFF;
  background-color: #ffffff;
}

.input-wrapper:focus-within .input-icon {
  color: #016BFF;
}

.password-toggle-btn {
  position: absolute;
  right: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
  width: 26px;
  height: 26px;
  background: transparent;
  border: none;
  color: #6b7280;
  border-radius: 6px;
  cursor: pointer;
  padding: 0;
  transition: color 0.2s ease;
}

.password-toggle-btn:hover {
  color: #111827;
}

.submit-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  width: 100%;
  height: 46px;
  margin-top: auto;
  background-color: #016BFF;
  color: #ffffff;
  border: none;
  border-radius: 12px;
  font-size: 14.5px;
  font-weight: 700;
  cursor: pointer;
  outline: none;
  font-family: inherit;
  transition: 
    background-color 0.2s ease, 
    transform 0.15s ease;
}

.submit-btn:hover:not(:disabled) {
  background-color: #005ae0;
}

.submit-btn:active:not(:disabled) {
  transform: scale(0.98);
}

.submit-btn:disabled {
  opacity: 0.7;
  cursor: not-allowed;
}

.spin-icon {
  animation: spin 0.9s linear infinite;
}

@keyframes spin {
  from {
    transform: rotate(0deg);
  }
  to {
    transform: rotate(360deg);
  }
}

.form-footer {
  margin-top: 14px;
  min-height: 26px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.footer-content {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 6px;
}

.footer-text {
  font-size: 13px;
  color: #6b7280;
  font-weight: 500;
}

.link-btn {
  background: none;
  border: none;
  color: #016BFF;
  font-weight: 700;
  font-size: 13px;
  cursor: pointer;
  padding: 0;
  font-family: inherit;
  text-decoration: underline;
  text-underline-offset: 3px;
  transition: color 0.15s ease;
}

.link-btn:hover {
  color: #004ec2;
}

.sub-fade-enter-active,
.sub-fade-leave-active {
  transition: opacity 0.18s ease, transform 0.18s ease;
}

.sub-fade-enter-from {
  opacity: 0;
  transform: translateY(4px);
}

.sub-fade-leave-to {
  opacity: 0;
  transform: translateY(-4px);
}

.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.2s ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}
</style>
