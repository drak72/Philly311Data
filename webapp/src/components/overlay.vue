<script setup lang="ts">

defineProps<{
    isLoading: boolean
}>();

</script>

<template>
    <div class="loading-overlay" v-if="isLoading">
      <div class="spinner-container">
        <img src="/favicon.png" alt="Loading..." class="spinning-logo" />
      </div>
    </div>
</template>

<style scoped>
/* Loading Overlay Styles */
.loading-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: rgba(255, 255, 255, 0.8);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 1000;
}


.spinner-container {
  width: 100px;
  height: 100px;
  display: flex;
  justify-content: center;
  align-items: center;
}

.spinning-logo {
  width: 100%;
  height: 100%;
  object-fit: contain;
  animation: spin 1.5s linear infinite;
}

@keyframes spin {
  from {
    transform: rotate(0deg);
  }
  to {
    transform: rotate(360deg);
  }
}
</style>