<script setup lang="ts">
import { ref } from 'vue';
import Input from './components/input.vue';

const question = ref('');
const answer = ref('');
const isLoading = ref(false);

const handleInput = (value: string) => {
  question.value = value;
}

const handleSubmit = async () => {
  if (!question.value.trim()) return; // Prevent empty submissions
  isLoading.value = true;
  
  try {
    const response = await fetch('http://localhost:5211/?question=' + question.value, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json'
      },
    });

    answer.value = await response.json();
  } catch (error) {
    console.error('Error:', error);
  } finally {
    isLoading.value = false;
  }
}
</script>

<template>
  <div class="container" @keydown.enter="handleSubmit">
    <!-- Loading Overlay -->
    <div class="loading-overlay" v-if="isLoading">
      <div class="spinner-container">
        <img src="/favicon.png" alt="Loading..." class="spinning-logo" />
      </div>
    </div>

    <h1 class="title">Open Data Philly Queries</h1>
    <h3 class="subtitle">Ask a question about the City of Philadelphia's 311 data</h3>

    <!--  -->
    <div class="input-container">
      <Input 
        placeholder="Enter your question" 
        @update:inputValue="handleInput" 
        :disabled="isLoading"
      />

      <button 
        @click="handleSubmit" 
        :disabled="isLoading"
        class="submit-button"
      >
        Submit
      </button>
    </div>
    
    <!-- Display the answer -->
    <div class="card" v-if="answer">
      <div v-for="(item, index) in answer" :key="index" class="answer-item">
        {{ item }}
      </div>
    </div>
  </div>
</template>

<style scoped>
.container {
  max-width: 800px;
  margin: 0 auto;
  padding: 2rem;
  position: relative;
  min-height: 100vh;
}

.title {
  padding-top: 5rem;
  font-size: 2.5rem;
  margin-bottom: 1rem;
}

.subtitle {
  color: #535357;
  margin-bottom: 2rem;
  font-weight: 500;
}

.input-container {
  padding: 5rem 0;
  display: flex;
  align-items: center;
  gap: 1rem;
  margin-bottom: 2rem;
}

.input-container input {
  border-radius: 10px;
  border: 1px solid #333;
  padding: 0.5rem 1.5rem;
  width: 100%;
  height: 100%;
}

.submit-button {
  padding: 0.45rem 1.5rem;
  color: white;
  border: 1px solid #333;
  border-radius: 10px;
  cursor: pointer;
  transition: background-color 0.3s;
}

.submit-button:hover {
  background-color: #5a67dd;
}

.submit-button:disabled {
  background-color: #797474;
  cursor: not-allowed;
}

.card {
  background: #1a1818;
  border-radius: 10px;
  border: 1px solid #333;
  padding: 1.5rem;
  margin-top: 2rem;
  color: #fff;
  justify-content: left;
}

.answer-item {
  display: list-item;
  list-style-type: circle;
  list-style-position: outside;
  text-align: left;
  overflow-wrap: break-word;
  margin-left: 1rem;
  margin-bottom: 0.5rem;
}


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