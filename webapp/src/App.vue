<script setup lang="ts">
import { ref } from 'vue';
import Input from './components/input.vue';
import Overlay from './components/overlay.vue';
import AnswerCard from './components/answercard.vue';

const question = ref('');
let answer = ref([] as string[]);

const isLoading = ref(false);

const handleInput = (value: string) => {
  question.value = value;
}

//ToDo: Genericize this
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
    answer.value = ["Sorry, we've hit an error!", JSON.stringify(error)];
  } finally {
    isLoading.value = false;
  }
}
</script>

<template>
  <div class="container" @keydown.enter="handleSubmit">
    <!-- Loading Overlay -->
    <Overlay :isLoading="isLoading" />

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
    
    <!-- Display the answer card -->
    <AnswerCard :answer="answer" />
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



</style>